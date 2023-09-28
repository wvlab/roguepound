﻿namespace FunctionalRoguePound

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


module FUtility =
    let BresenhamLine (x1: int) (y1: int) (x2: int) (y2: int) =
        let dx = abs (x2 - x1)
        let dy = abs (y2 - y1)
        let sx = if x1 < x2 then 1 else -1
        let sy = if y1 < y2 then 1 else -1
        let mutable err = dx - dy
        let mutable x = x1
        let mutable y = y1

        Seq.unfold
            (fun _ ->
                if x = x2 && y = y2 then
                    None
                else
                    let e2 = 2 * err
                    let x' = if e2 > -dy then x + sx else x
                    let y' = if e2 < dx then y + sy else y
                    let err' = if e2 > -dy && e2 < dx then err + dx - dy else err
                    x <- x'
                    y <- y'
                    err <- err'
                    Some((x, y), ()))
            ()

    let EuclideanDistance (x1: double) (y1: double) (x2: double) (y2: double) =
        let x' = x2 - x1
        let y' = y2 - y1

        sqrt (x' * x' + y' * y')
