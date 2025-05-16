package player

import (
	"github.com/heroiclabs/nakama-common/runtime"
	"maps"
	"math/rand"
	"shut-the-box-server/api"
	"shut-the-box-server/jokers"
	"slices"
)

type Player api.Player

func NewPlayer(presence runtime.Presence, tileCount int, diceCount int, roundCount int) *api.Player {
	player := &api.Player{
		PlayerId: presence.GetUserId(),
		State:    api.PlayerState_IDLE,
		Score:    0,
		Rolls:    make([]int32, diceCount),
		Tiles:    make([]api.TileState, tileCount),
		Jokers:   make([]api.Joker, 0, roundCount),
	}
	for t := 0; t < tileCount; t++ {
		player.Tiles[t] = api.TileState_OPEN
	}
	return player
}

func (p *Player) Reset() {
	p.State = api.PlayerState_IDLE
	for t := 0; t < len(p.Tiles); t++ {
		p.Tiles[t] = api.TileState_OPEN
	}
}

func (p *Player) Toggle(index int) api.TileState {
	switch p.Tiles[index] {
	case api.TileState_OPEN:
		p.Tiles[index] = api.TileState_TOGGLE
	case api.TileState_TOGGLE:
		p.Tiles[index] = api.TileState_OPEN
	case api.TileState_SHUT:
	}
	return p.Tiles[index]
}

func (p *Player) RollDice(rand *rand.Rand) {
	for i := range p.Rolls {
		p.Rolls[i] = rand.Int31n(5) + 1
	}

	if p.HasMoves() {
		p.State = api.PlayerState_PLAY
	} else {
		p.State = api.PlayerState_FAIL
	}
}

func (p *Player) CanConfirm() bool {
	sum := 0
	for i, tile := range p.Tiles {
		if tile == api.TileState_TOGGLE {
			sum += i + 1
		}
	}
	return int(p.TotalRoll()) == sum
}

func (p *Player) Confirm() int32 {
	score := int32(0)
	for i, tile := range p.Tiles {
		if tile != api.TileState_TOGGLE {
			continue
		}
		p.Tiles[i] = api.TileState_SHUT
		score = int32(i + 1)
	}
	p.State = api.PlayerState_IDLE
	return score
}

func (p *Player) Fail() {
	p.State = api.PlayerState_FAIL
}

func (p *Player) Revert() {
	for i, tile := range p.Tiles {
		if tile != api.TileState_TOGGLE {
			continue
		}
		p.Tiles[i] = api.TileState_OPEN
	}
}

func (p *Player) TotalRoll() int32 {
	sum := int32(0)
	for _, roll := range p.Rolls {
		sum += roll
	}
	return sum
}

func (p *Player) BoxShut() bool {
	for _, tile := range p.Tiles {
		if tile == api.TileState_OPEN || tile == api.TileState_TOGGLE {
			return false
		}
	}
	p.State = api.PlayerState_DONE
	return true
}

func (p *Player) HasMoves() bool {
	return canMakeSum(p.Tiles, int(p.TotalRoll()), 0)
}

func (p *Player) GetJokerChoices(rand *rand.Rand) []api.Joker {
	possible := make([]api.Joker, 0, len(jokers.JokerMap))
	for j := range maps.Keys(jokers.JokerMap) {
		if slices.Contains(p.Jokers, j) {
			continue
		}
		possible = append(possible, j)
	}
	rand.Shuffle(len(possible), func(i, j int) { possible[i], possible[j] = possible[j], possible[i] })

	choices := make([]api.Joker, 0, 3)
	for _, j := range possible {
		choices = append(choices, j)
		if len(choices) == 3 {
			break
		}
	}
	return choices
}

func canMakeSum(s []api.TileState, t int, i int) bool {
	if t == 0 {
		return true
	}
	if i >= len(s) || t < 0 {
		return false
	}
	n := i + 1
	v := i + 1
	return (s[i] != api.TileState_SHUT && canMakeSum(s, t-v, n)) || canMakeSum(s, t, n)
}
