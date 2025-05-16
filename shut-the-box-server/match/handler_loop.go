package match

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	"shut-the-box-server/api"
	"shut-the-box-server/player"
)

func (m Handler) MatchLoop(_ context.Context, logger runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, dispatcher runtime.MatchDispatcher, _ int64, state interface{}, messages []runtime.MatchData) interface{} {
	s := state.(*State)
	if s.ConnectedCount()+s.joinsInProgress == 0 {
		s.emptyTicks++
		if s.emptyTicks >= maxEmptySec*tickRate {
			return nil
		}
	}

	m.processMessages(logger, dispatcher, s, messages)

	switch s.matchState {
	case Starting:
		m.matchStarting(logger, dispatcher, s)
	case Waiting:
		m.matchWaiting(logger, dispatcher, s)
	case Choosing:
		m.roundStarting(logger, dispatcher, s)
	case Playing:
		m.matchPlaying(logger, dispatcher, s)
	}

	return s
}

func (m Handler) matchStarting(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *State) {
	if len(state.presences) < 2 {
		return
	}

	index := 0
	for _, v := range state.presences {
		state.players[index] = player.NewPlayer(v, tileCount, diceCount, roundCount)
		index++
	}

	if buf, err := m.Marshaler.Marshal(&api.MatchStart{
		Players:    state.players,
		RoundCount: int32(roundCount),
		TileCount:  int32(tileCount),
		TurnTime:   int32(turnTime),
		RoundId:    int32(0),
	}); err == nil {
		_ = dispatcher.BroadcastMessage(int64(api.OpCode_MATCH_START), buf, nil, nil, true)
	}

	state.matchState = Waiting
}

func (m Handler) matchWaiting(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *State) {
	if state.playerReady >= len(state.players) {
		state.matchState = Choosing
		state.pauseTicks = chooseTime * tickRate
	}
}

func (m Handler) roundStarting(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *State) {
	if state.pauseTicks == chooseTime*tickRate {
		choices := make([]*api.JokerChoice, playerCount)
		state.waitingSelect = 0
		for i, p := range state.players {
			cu := (*player.Player)(p)
			jokerChoices := cu.GetJokerChoices(state.random)
			if len(jokerChoices) > 0 {
				state.waitingSelect++
			}
			choices[i] = &api.JokerChoice{
				PlayerId: cu.PlayerId,
				Jokers:   jokerChoices,
			}
		}
		if buf, err := m.Marshaler.Marshal(&api.RoundStart{
			RoundId: int32(state.roundId),
			Choices: choices,
		}); err == nil {
			_ = dispatcher.BroadcastMessage(int64(api.OpCode_ROUND_START), buf, nil, nil, true)
		}
	}
	if state.pauseTicks > 0 {
		state.pauseTicks--
	}
	if state.pauseTicks == 0 || state.waitingSelect <= 0 {
		state.matchState = Playing
		m.sendTurn(dispatcher, state)
	}
}

func (m Handler) matchPlaying(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *State) {

}

func (m Handler) processMessages(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *State, messages []runtime.MatchData) {
	for _, message := range messages {
		player := state.GetPlayer(message)
		switch api.OpCode(message.GetOpCode()) {
		case api.OpCode_PLAYER_READY:
			state.playerReady++

		case api.OpCode_PLAYER_SELECT:
			state.waitingSelect--
			jSelect := &api.JokerSelect{}
			_ = m.Unmarshaler.Unmarshal(message.GetData(), jSelect)
			jSelect.PlayerId = player.PlayerId
			player.Jokers = append(player.Jokers, jSelect.Selected)
			if buf, err := m.Marshaler.Marshal(jSelect); err == nil {
				_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_SELECT), buf, nil, nil, true)
			}

		case api.OpCode_PLAYER_ROLL:
			rollResponse := player.RollResponse(state.random)
			if rollResponse != nil {
				if buf, err := m.Marshaler.Marshal(rollResponse); err == nil {
					_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_ROLL), buf, nil, nil, true)
				}
			}

		case api.OpCode_PLAYER_MOVE:
			move := &api.PlayerMove{}
			_ = m.Unmarshaler.Unmarshal(message.GetData(), move)
			move.PlayerId = player.PlayerId
			move.State = player.Toggle(int(move.Index))
			if buf, err := m.Marshaler.Marshal(move); err == nil {
				_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_MOVE), buf, nil, nil, true)
			}

		case api.OpCode_PLAYER_CONF:
			confirm := player.ConfirmResponse()
			if confirm != nil {
				if buf, err := m.Marshaler.Marshal(confirm); err == nil {
					_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_CONF), buf, nil, nil, true)
				}
				m.advancePlayer(dispatcher, state)
			}
		case api.OpCode_PLAYER_FAIL:
			player.Fail()
			m.advancePlayer(dispatcher, state)
		}
	}
}

func (m Handler) advancePlayer(dispatcher runtime.MatchDispatcher, state *State) {
	if state.NextPlayer() {
		m.sendTurn(dispatcher, state)
	} else {
		m.roundFinished(dispatcher, state)
	}
}

func (m Handler) sendTurn(dispatcher runtime.MatchDispatcher, state *State) {
	player := state.GetPlayer(nil)
	player.State = api.PlayerState_ROLL
	if buf, err := m.Marshaler.Marshal(&api.PlayerTurn{
		PlayerId: player.PlayerId,
	}); err == nil {
		_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_TURN), buf, nil, nil, true)
	}
}

func (m Handler) roundFinished(dispatcher runtime.MatchDispatcher, state *State) {
	state.roundId += 1

	for _, p := range state.players {
		(*player.Player)(p).Reset()
	}
	if state.roundId == roundCount {
		m.matchFinished(dispatcher, state)
	} else {
		state.matchState = Choosing
		state.pauseTicks = chooseTime * tickRate
	}
}

func (m Handler) matchFinished(dispatcher runtime.MatchDispatcher, state *State) {
	state.matchState = Complete

	scores := make([]*api.PlayerScore, len(state.players))
	for i, player := range state.players {
		scores[i] = &api.PlayerScore{
			PlayerId: player.PlayerId,
			Score:    player.Score,
		}
	}
	if buf, err := m.Marshaler.Marshal(&api.MatchOver{
		Scores: scores,
	}); err == nil {
		_ = dispatcher.BroadcastMessage(int64(api.OpCode_MATCH_OVER), buf, nil, nil, true)
	}
}
