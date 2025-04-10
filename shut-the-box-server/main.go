package main

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	_ "google.golang.org/protobuf/encoding/prototext"
	"google.golang.org/protobuf/proto"
	_ "google.golang.org/protobuf/proto"
	"time"
)

var (
	errInternalError  = runtime.NewError("internal server error", 13) // INTERNAL
	errMarshal        = runtime.NewError("cannot marshal type", 13)   // INTERNAL
	errNoInputAllowed = runtime.NewError("no input allowed", 3)       // INVALID_ARGUMENT
	errNoUserIdFound  = runtime.NewError("no user ID in context", 3)  // INVALID_ARGUMENT
	errUnmarshal      = runtime.NewError("cannot unmarshal type", 13) // INTERNAL
)

// noinspection GoUnusedExportedFunction
func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	initStart := time.Now()

	marshaler := &proto.MarshalOptions{}
	unmarshaler := &proto.UnmarshalOptions{}

	if err := initializer.RegisterMatch(moduleName, func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule) (runtime.Match, error) {
		return &MatchHandler{
			marshaler:   marshaler,
			unmarshaler: unmarshaler,
		}, nil
	}); err != nil {
		return err
	}

	if err := initializer.RegisterMatchmakerMatched(func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, entries []runtime.MatchmakerEntry) (string, error) {
		return nk.MatchCreate(ctx, moduleName, entries[0].GetProperties())
	}); err != nil {
		return err
	}

	logger.Info("Module loaded in %dms", time.Since(initStart).Nanoseconds())
	return nil
}
