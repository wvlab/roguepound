namespace FunctionalRoguePound

open System

module FUtility =
    let BresenhamLine (x1: int) (y1: int) (x2: int) (y2: int) =
        let dx = abs (x2 - x1)
        let dy = abs (y2 - y1)
        let sx = if x1 < x2 then 1 else -1
        let sy = if y1 < y2 then 1 else -1
        let err = dx - dy

        let aux (x, y, err) =
            if (x = x2 && y = y2) || (x * sx > x2 * sx) then
                None
            else
                let e2 = 2 * err
                let x' = if e2 > -dy then x + sx else x
                let y' = if e2 < dx then y + sy else y
                let err' = if e2 > -dy && e2 < dx then err + dx - dy else err
                Some(struct (x, y), (x', y', err'))

        Seq.unfold aux (x1, y1, err)

    let EuclideanDistance (x1: double) (y1: double) (x2: double) (y2: double) =
        let x' = x2 - x1
        let y' = y2 - y1

        sqrt (x' * x' + y' * y')

    let BoundInt (lower: int) (upper: int) (x: int) = min upper (max lower x)
