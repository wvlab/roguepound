namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility

type IActor =
    abstract member Letter: string
    abstract member Position: Position with get, set

type IMonster =
    inherit IActor
    abstract member Behavior: Behavior

and Behavior = | Basic
// | Distant
// | Ghost

type Player() =
    interface IActor with
        member val Letter = "@"
        member val Position: Position = { X = 0; Y = 0 } with get, set

    member public this.Letter = (this :> IActor).Letter
    member public this.Position = (this :> IActor).Position
