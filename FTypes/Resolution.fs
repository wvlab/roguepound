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
