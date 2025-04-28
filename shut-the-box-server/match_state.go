package main

import (
	"github.com/heroiclabs/nakama-common/runtime"
	"math/rand"
	"shut-the-box-server/api"
	"time"
)

type MatchStatus int

const (
	Waiting MatchStatus = iota
	Starting
	Playing
	Complete
)

type MatchState struct {
	presences       map[string]runtime.Presence
	players         []*api.Player
	rounds          []*api.RoundScore
	matchState      MatchStatus
	random          *rand.Rand
	playerIndex     int
	readyCount      int
	joinsInProgress int
	emptyTicks      int
	pauseTicks      int
}

func NewMatchState(playerCount int) *MatchState {
	return &MatchState{
		presences:   make(map[string]runtime.Presence, playerCount),
		players:     make([]*api.Player, playerCount),
		rounds:      make([]*api.RoundScore, 0, roundCount),
		random:      rand.New(rand.NewSource(time.Now().UnixNano())),
		emptyTicks:  0,
		playerIndex: 0,
	}
}

func (ms *MatchState) ConnectedCount() int {
	count := 0
	for _, p := range ms.presences {
		if p != nil {
			count++
		}
	}
	return count
}

func (ms *MatchState) NextPlayer() bool {
	idleIndex := -1
	for i := 1; i < len(ms.players)+1; i++ {
		index := (ms.playerIndex + i) % len(ms.players)
		if ms.players[index].State != api.PlayerState_IDLE {
			continue
		}
		idleIndex = index
		break
	}

	if idleIndex == -1 {
		return false
	} else {
		ms.playerIndex = idleIndex
		return true
	}
}

func (ms *MatchState) GetPlayer(presence runtime.Presence) *Player {
	if presence == nil {
		return (*Player)(ms.players[ms.playerIndex])
	} else {
		playerId := presence.GetUserId()
		var player *api.Player = nil
		for _, p := range ms.players {
			if p.PlayerId == playerId {
				player = p
			}
		}
		return (*Player)(player)
	}
}

func (ms *MatchState) GetRoundScore() *api.RoundScore {
	players := make([]string, len(ms.players))
	scores := make([]int32, len(ms.players))
	for i, p := range ms.players {
		players[i] = p.PlayerId
		scores[i] = int32((*Player)(p).GetScore())
	}
	return &api.RoundScore{
		Players: players,
		Scores:  scores,
	}
}

func (ms *MatchState) GetRoll() int {
	return ms.random.Intn(11) + 2
}
