package main

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	"google.golang.org/protobuf/proto"
)

const (
	moduleName    = "shut-the-box"
	maxEmptySec   = 30
	roundInterval = 5
	tickRate      = 5
	playerCount   = 2
	roundCount    = 2
	tileCount     = 9
	turnTime      = 0
)

var _ runtime.Match = &MatchHandler{}

type MatchHandler struct {
	marshaler   *proto.MarshalOptions
	unmarshaler *proto.UnmarshalOptions
}

func (m MatchHandler) MatchInit(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ map[string]interface{}) (interface{}, int, string) {
	state := NewMatchState(playerCount)
	return state, tickRate, ""
}

func (m MatchHandler) MatchJoinAttempt(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ runtime.MatchDispatcher, _ int64, state interface{}, presence runtime.Presence, _ map[string]string) (interface{}, bool, string) {
	s := state.(*MatchState)

	if presence, ok := s.presences[presence.GetUserId()]; ok {
		if presence == nil {
			return s, true, ""
		} else {
			return s, false, "already joined"
		}
	}

	if len(s.presences)+s.joinsInProgress >= playerCount {
		return s, false, "match full"
	}

	s.joinsInProgress++
	return s, true, ""
}

func (m MatchHandler) MatchJoin(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ runtime.MatchDispatcher, _ int64, state interface{}, presences []runtime.Presence) interface{} {
	s := state.(*MatchState)

	for _, presence := range presences {
		s.emptyTicks = 0
		s.presences[presence.GetUserId()] = presence
		s.joinsInProgress--
	}
	return s
}

func (m MatchHandler) MatchLeave(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ runtime.MatchDispatcher, _ int64, state interface{}, presences []runtime.Presence) interface{} {
	s := state.(*MatchState)
	for _, presence := range presences {
		s.presences[presence.GetUserId()] = nil
	}
	if len(s.presences)+s.joinsInProgress == 0 {
		return nil
	}

	return s
}

func (m MatchHandler) MatchTerminate(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ runtime.MatchDispatcher, _ int64, state interface{}, _ int) interface{} {
	return state
}

func (m MatchHandler) MatchSignal(_ context.Context, _ runtime.Logger, _ *sql.DB, _ runtime.NakamaModule, _ runtime.MatchDispatcher, _ int64, state interface{}, _ string) (interface{}, string) {
	return state, ""
}
