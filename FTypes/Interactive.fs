namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility

type InteractiveObjectType =
    | Coins of amount: int
    | Stairs
    | Pickup

[<Struct>]
type InteractiveObject =
    { Type: InteractiveObjectType
      Position: Position }

    member this.Letter =
        match this.Type with
        | Coins _ -> "*"
        | Stairs -> "%"
        | Pickup -> "&"
