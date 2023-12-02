namespace FunctionalRoguePound

open Raylib_CsLo
open System.Numerics
open FunctionalRoguePound.FUtility

type TileType =
    | LTCorner
    | RTCorner
    | LBCorner
    | RBCorner
    | HTWall
    | HBWall
    | VLWall
    | VRWall
    | Floor
    | Path
    | TDoor
    | BDoor
    | LDoor
    | RDoor
    | Void

[<Struct>]
type public Tile =
    { mutable Type: TileType
      mutable IsOpen: bool }

type ITileSet =
    abstract DrawTile: Tile -> Vector2 -> unit

module private CommonThings =
    let checkvoid (func: (unit -> unit)) =
        function
        | Void -> ()
        | _ -> func ()

type TestingTileSet() =
    interface ITileSet with
        member this.DrawTile tile destination =
            let color =
                match tile.Type with
                | LTCorner -> Raylib.LIGHTGRAY
                | RTCorner -> Raylib.DARKGRAY
                | LBCorner -> Raylib.YELLOW
                | RBCorner -> Raylib.ORANGE
                | HTWall
                | HBWall -> Raylib.RED
                | VLWall
                | VRWall -> Raylib.MAROON
                | Floor -> Raylib.RAYWHITE
                | Path -> Raylib.WHITE
                | LDoor
                | TDoor -> Raylib.BROWN
                | RDoor
                | BDoor -> Raylib.DARKBROWN
                | _ -> Raylib.MAGENTA

            let draw () =
                Raylib.DrawRectangleV(destination, Vector2(float32 Settings.TileSize, float32 Settings.TileSize), color)

            CommonThings.checkvoid draw (tile.Type)


type ClassicTileSet() =
    // TODO: FIX PLACE WHERE IT'S WRITTEN
    interface ITileSet with
        member this.DrawTile tile destination =
            let text =
                match tile.Type with
                | LTCorner
                | RTCorner
                | LBCorner
                | RBCorner -> "+"
                | HTWall
                | HBWall -> "--"
                | VLWall
                | VRWall -> "|"
                | Floor -> "."
                | Path -> "#"
                | TDoor -> "/"
                | BDoor -> "\\"
                | LDoor -> "\\"
                | RDoor -> "/"
                | _ -> "?"


            let draw = FDraw.TextCentered text destination Settings.TileSize Raylib.WHITE

            CommonThings.checkvoid draw (tile.Type)
