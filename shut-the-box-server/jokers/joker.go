package jokers

import "shut-the-box-server/api"

type Joker interface {
	TryApply(rolls []int32, tiles []api.TileState) int32
}

var JokerMap map[api.Joker]Joker = map[api.Joker]Joker{
	api.Joker_DOUBLE_DICE: &DoubleDice{},
	api.Joker_ODD_TILE:    &OddTile{},
	api.Joker_EVEN_TILE:   &EvenTile{},
}
