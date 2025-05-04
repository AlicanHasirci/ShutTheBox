package main

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	"shut-the-box-server/api"
)

func (m MatchHandler) MatchLoop(_ context.Context, logger runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, dispatcher runtime.MatchDispatcher, _ int64, state interface{}, messages []runtime.MatchData) interface{} {
	s := state.(*MatchState)
	if s.ConnectedCount()+s.joinsInProgress == 0 {
		s.emptyTicks++
		if s.emptyTicks >= maxEmptySec*tickRate {
			return nil
		}
	}

	m.processMessages(logger, dispatcher, s, messages)

	switch s.matchState {
	case Waiting:
		m.matchWaiting(logger, dispatcher, s)
	case Starting:
		m.matchStarting(logger, dispatcher, s)
	case Playing:
		m.matchPlaying(logger, dispatcher, s)
	}

	return s
}

func (m MatchHandler) matchWaiting(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *MatchState) {
	if len(state.presences) < 2 {
		return
	}

	index := 0
	for _, v := range state.presences {
		state.players[index] = NewPlayer(v, tileCount)
		index++
	}

	if buf, err := m.marshaler.Marshal(&api.MatchStart{
		Players:    state.players,
		RoundCount: int32(roundCount),
		TileCount:  int32(tileCount),
		TurnTime:   int32(turnTime),
	}); err == nil {
		_ = dispatcher.BroadcastMessage(int64(api.OpCode_MATCH_START), buf, nil, nil, true)
	}

	state.matchState = Starting
	state.pauseTicks = roundInterval * tickRate
}

func (m MatchHandler) matchStarting(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *MatchState) {
	if state.readyCount < playerCount {
		return
	}
	if state.pauseTicks == roundInterval*tickRate {
		var score *api.RoundScore
		if len(state.rounds) == 0 {
			score = nil
		} else {
			score = state.rounds[len(state.rounds)-1]
		}
		if buf, err := m.marshaler.Marshal(&api.RoundStart{
			Interval: int32(roundInterval),
			Score:    score,
		}); err == nil {
			_ = dispatcher.BroadcastMessage(int64(api.OpCode_ROUND_START), buf, nil, nil, true)
		}
	}
	if state.pauseTicks > 0 {
		state.pauseTicks--
	}
	if state.pauseTicks == 0 {
		state.matchState = Playing
		m.sendTurn(dispatcher, state)
	}
}

func (m MatchHandler) matchPlaying(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *MatchState) {

}

func (m MatchHandler) processMessages(logger runtime.Logger, dispatcher runtime.MatchDispatcher, state *MatchState, messages []runtime.MatchData) {
	for _, message := range messages {
		player := state.GetPlayer(message)
		switch api.OpCode(message.GetOpCode()) {
		case api.OpCode_PLAYER_READY:
			state.readyCount++
		case api.OpCode_PLAYER_ROLL:
			reRoll := player.State == api.PlayerState_PLAY || player.State == api.PlayerState_FAIL
			if player.State == api.PlayerState_ROLL || reRoll {
				if reRoll {
					// This is a reRoll
				}
				player.RollDice(state)
				if buf, err := m.marshaler.Marshal(&api.PlayerRoll{
					PlayerId: player.PlayerId,
					Roll:     player.Roll,
				}); err == nil {
					_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_ROLL), buf, nil, nil, true)
				}
			}

		case api.OpCode_PLAYER_MOVE:
			move := &api.PlayerMove{}
			_ = m.unmarshaler.Unmarshal(message.GetData(), move)
			move.PlayerId = player.PlayerId
			move.State = player.Toggle(int(move.Index))
			if buf, err := m.marshaler.Marshal(move); err == nil {
				_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_MOVE), buf, nil, nil, true)
			}

		case api.OpCode_PLAYER_CONF:
			if player.TryConfirm() {
				if buf, err := m.marshaler.Marshal(&api.PlayerConfirm{
					PlayerId: player.PlayerId,
					Tiles:    player.Tiles,
					BoxShut:  player.BoxShut(),
				}); err == nil {
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

func (m MatchHandler) advancePlayer(dispatcher runtime.MatchDispatcher, state *MatchState) {
	if state.NextPlayer() {
		m.sendTurn(dispatcher, state)
	} else {
		m.roundFinished(dispatcher, state)
	}
}

func (m MatchHandler) sendTurn(dispatcher runtime.MatchDispatcher, state *MatchState) {
	player := state.GetPlayer(nil)
	player.State = api.PlayerState_ROLL
	if buf, err := m.marshaler.Marshal(&api.PlayerTurn{
		PlayerId: player.PlayerId,
	}); err == nil {
		_ = dispatcher.BroadcastMessage(int64(api.OpCode_PLAYER_TURN), buf, nil, nil, true)
	}
}

func (m MatchHandler) roundFinished(dispatcher runtime.MatchDispatcher, state *MatchState) {
	score := state.GetRoundScore()
	state.rounds = append(state.rounds, score)
	for _, p := range state.players {
		(*Player)(p).Reset()
	}
	if len(state.rounds) == roundCount {
		state.matchState = Complete
		if buf, err := m.marshaler.Marshal(&api.MatchOver{
			Winner: state.players[0].PlayerId, //FIXME
			Rounds: state.rounds,
		}); err == nil {
			_ = dispatcher.BroadcastMessage(int64(api.OpCode_MATCH_OVER), buf, nil, nil, true)
		}
	} else {
		state.matchState = Starting
		state.pauseTicks = roundInterval * tickRate
	}
}
