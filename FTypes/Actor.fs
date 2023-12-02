namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility

[<Struct>]
type Stats =
    { mutable Health: int
      mutable MaxHealth: int
      mutable Attack: int
      mutable Armor: int }

    static member Default =
        { Health = 10
          MaxHealth = 10
          Attack = 3
          Armor = 2 }

type IActor =
    abstract member Letter: string
    abstract member Position: Position with get, set
    abstract member Stats: Stats with get, set

type IMonster =
    inherit IActor
    abstract member Behavior: Behavior

and Behavior = | Basic
// | Distant
// | Ghost

type Player() =
    member val Level = 1 with get, set
    member val Experience = 0 with get, set
    member val ExperienceCap = 7 with get, set

    interface IActor with
        member val Letter = "@"
        member val Position: Position = { X = 0; Y = 0 } with get, set

        member val Stats: Stats =
            { Stats.Default with
                MaxHealth = 12
                Health = 12 } with get, set

    member public this.Letter = (this :> IActor).Letter
    member public this.Position = (this :> IActor).Position
    member public this.Stats = (this :> IActor).Stats
