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
	matchState      MatchStatus
	random          *rand.Rand
	roundId         int
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
		random:      rand.New(rand.NewSource(time.Now().UnixNano())),
		roundId:     0,
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

func (ms *MatchState) GetRoll() int32 {
	return ms.random.Int31n(5) + 1
}
