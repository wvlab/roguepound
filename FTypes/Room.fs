namespace FunctionalRoguePound

[<Struct>]
type public Room =
    { mutable x1: int
      mutable y1: int
      mutable x2: int
      mutable y2: int }

    static member WallOffset: int = 2

    member this.HWallXCoords() =
        seq { this.x1 + Room.WallOffset + 1 .. this.x2 - Room.WallOffset - 1 }

    member this.VWallYCoords() =
        seq { this.y1 + Room.WallOffset + 1 .. this.y2 - Room.WallOffset - 1 }

    member this.IsInRoom(x: int, y: int) =
        Seq.exists ((=) x) (this.HWallXCoords())
        && Seq.exists ((=) y) (this.VWallYCoords())

    member this.Center() =
        let aux r1 r2 = float32 r1 + float32 (r2 - r1) / 2.0f

        struct (aux this.x1 this.x2, aux this.y1 this.y2)

    member this.Area() : int64 =
        int64 (this.x2 - this.x1 - 2 * Room.WallOffset)
        * int64 (this.y2 - this.y1 - 2 * Room.WallOffset)

    member this.RandPos(rng: System.Random) =
        let aux r1 r2 =
            rng.Next(r1 + Room.WallOffset + 1, r2 - Room.WallOffset)

        struct (aux this.x1 this.x2, aux this.y1 this.y2)

    static member op_Equality(r1: Room, r2: Room) = r1.Equals r2
