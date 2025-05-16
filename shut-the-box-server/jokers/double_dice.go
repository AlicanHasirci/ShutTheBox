package jokers

import (
	"shut-the-box-server/api"
)

var _ Joker = &DoubleDice{}

type DoubleDice struct{}

func (d DoubleDice) TryApply(rolls []int32, _ []api.TileState) int32 {

	if len(rolls) == 0 {
		return 0
	}
	roll := rolls[0]
	sum := int32(0)
	for _, r := range rolls {
		if roll != r {
			return 0
		}
		sum += r
	}

	return sum
}
