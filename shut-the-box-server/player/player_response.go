package player

import (
	"math/rand"
	"shut-the-box-server/api"
	"shut-the-box-server/jokers"
)

func (p *Player) ConfirmResponse() *api.PlayerConfirm {
	appliedJokers := make([]*api.JokerScore, len(p.Jokers))
	if p.CanConfirm() {
		totalScore := int32(0)
		for _, j := range p.Jokers {
			joker := jokers.JokerMap[j]
			if jScore := joker.TryApply(p.Rolls, p.Tiles); jScore > 0 {
				appliedJokers = append(appliedJokers, &api.JokerScore{
					Joker: j,
					Score: jScore,
				})
				totalScore += jScore
			}
		}
		totalScore += p.Confirm()
		p.Score += totalScore
		return &api.PlayerConfirm{
			PlayerId: p.PlayerId,
			Tiles:    p.Tiles,
			BoxShut:  p.BoxShut(),
			Score:    totalScore,
			Jokers:   appliedJokers,
		}
	} else {
		return nil
	}
}

func (p *Player) RollResponse(rand *rand.Rand) *api.PlayerRoll {
	reRoll := p.State == api.PlayerState_PLAY || p.State == api.PlayerState_FAIL
	if p.State == api.PlayerState_ROLL || reRoll {
		if reRoll {
			// This is a reRoll
		}
		p.RollDice(rand)
		return &api.PlayerRoll{
			PlayerId: p.PlayerId,
			Rolls:    p.Rolls,
		}
	} else {
		return nil
	}
}
