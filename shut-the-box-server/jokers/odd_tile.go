package jokers

import "shut-the-box-server/api"

var _ Joker = &OddTile{}

type OddTile struct{}

func (d OddTile) TryApply(_ []int32, tiles []api.TileState) int32 {

	sum := int32(0)
	for i, t := range tiles {
		val := i + 1
		if isOdd(val) && t == api.TileState_TOGGLE {
			sum += 1
		}
	}

	return sum
}

func isOdd(i int) bool {
	return i%2 == 1
}
