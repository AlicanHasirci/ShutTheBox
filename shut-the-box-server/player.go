package main

import (
	"github.com/heroiclabs/nakama-common/runtime"
	"shut-the-box-server/api"
)

type Player api.Player

func NewPlayer(presence runtime.Presence, tileCount int) *api.Player {
	player := &api.Player{
		PlayerId: presence.GetUserId(),
		State:    api.PlayerState_IDLE,
		Tiles:    make([]api.TileState, tileCount),
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

func (p *Player) RollDice(state *MatchState) {
	p.Roll = int32(state.GetRoll())
	if p.HasMoves() {
		p.State = api.PlayerState_PLAY
	} else {
		p.State = api.PlayerState_FAIL
	}
}

func (p *Player) TryConfirm() bool {
	sum := 0
	for i, tile := range p.Tiles {
		if tile == api.TileState_TOGGLE {
			sum += i + 1
		}
	}
	if int(p.Roll) == sum {
		for i, tile := range p.Tiles {
			if tile != api.TileState_TOGGLE {
				continue
			}
			p.Tiles[i] = api.TileState_SHUT
		}
		p.State = api.PlayerState_IDLE
		return true
	} else {
		return false
	}
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
	return canMakeSum(p.Tiles, int(p.Roll), 0)
}

func (p *Player) GetScore() int {
	sum := 0
	for i, tile := range p.Tiles {
		if tile == api.TileState_OPEN {
			sum += i + 1
		}
	}
	return sum
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
