namespace FunctionalRoguePound

open Raylib_CsLo

type Resolution =
    | Windowed of width: int * height: int
    | Borderless of width: int * height: int
    | Fullscreen

    member this.InitWindow(name: string) =
        let aux (flags: Option<ConfigFlags>) (width: int) (height: int) (name: string) =
            match flags with
            | Some(f) -> Raylib.SetConfigFlags(f)
            | None -> ()

            Raylib.InitWindow(width, height, name)

        match this with
        | Windowed(x, y) -> aux None x y name
        | Borderless(x, y) -> aux (Some ConfigFlags.FLAG_WINDOW_UNDECORATED) x y name
        | Fullscreen -> aux (Some ConfigFlags.FLAG_FULLSCREEN_MODE) 0 0 name


/// Class <c>Settings</c> keeps all constants and variables from settings file(in future)
type Settings() =
    static member val Resolution = Windowed(900, 600)

    static member TileSize = 32
    static member TileHeight = 48
    static member TileWidth = 80
    static member LevelCap = 21
