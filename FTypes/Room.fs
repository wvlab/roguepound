namespace FunctionalRoguePound

type public Room =
    struct
        val mutable public x1: int
        val mutable public y1: int
        val mutable public x2: int
        val mutable public y2: int
    end

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

    new(x1, y1, x2, y2) = { x1 = x1; y1 = y1; x2 = x2; y2 = y2 }
