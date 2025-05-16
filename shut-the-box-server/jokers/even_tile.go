package jokers

import "shut-the-box-server/api"

var _ Joker = &EvenTile{}

type EvenTile struct{}

func (d EvenTile) TryApply(_ []int32, tiles []api.TileState) int32 {

	sum := int32(0)
	for i, t := range tiles {
		val := i + 1
		if isEven(val) && t == api.TileState_TOGGLE {
			sum += 2
		}
	}

	return sum
}

func isEven(i int) bool {
	return i%2 == 0
}
