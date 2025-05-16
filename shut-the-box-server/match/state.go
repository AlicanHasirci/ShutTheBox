package match

import (
	"github.com/heroiclabs/nakama-common/runtime"
	"math/rand"
	"shut-the-box-server/api"
	"shut-the-box-server/player"
	"time"
)

type Status int

const (
	Starting Status = iota
	Waiting
	Choosing
	Playing
	Complete
)

type State struct {
	presences       map[string]runtime.Presence
	players         []*api.Player
	matchState      Status
	random          *rand.Rand
	roundId         int
	playerReady     int
	playerIndex     int
	waitingSelect   int
	joinsInProgress int
	emptyTicks      int
	pauseTicks      int
}

func NewMatchState(playerCount int) *State {
	return &State{
		presences:   make(map[string]runtime.Presence, playerCount),
		players:     make([]*api.Player, playerCount),
		random:      rand.New(rand.NewSource(time.Now().UnixNano())),
		roundId:     0,
		emptyTicks:  0,
		playerIndex: 0,
	}
}

func (ms *State) ConnectedCount() int {
	count := 0
	for _, p := range ms.presences {
		if p != nil {
			count++
		}
	}
	return count
}

func (ms *State) NextPlayer() bool {
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

func (ms *State) GetPlayer(presence runtime.Presence) *player.Player {
	if presence == nil {
		return (*player.Player)(ms.players[ms.playerIndex])
	} else {
		playerId := presence.GetUserId()
		var retVal *api.Player = nil
		for _, p := range ms.players {
			if p.PlayerId == playerId {
				retVal = p
			}
		}
		return (*player.Player)(retVal)
	}
}
