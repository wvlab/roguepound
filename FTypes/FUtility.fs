namespace FunctionalRoguePound

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
                    Some(struct (x, y), ()))
            ()

    let EuclideanDistance (x1: double) (y1: double) (x2: double) (y2: double) =
        let x' = x2 - x1
        let y' = y2 - y1

        sqrt (x' * x' + y' * y')

    let BoundInt (lower: int) (upper: int) (x: int) = min upper (max lower x)
