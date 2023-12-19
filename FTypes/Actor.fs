namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility

[<Struct>]
type Stats =
    { mutable Health: int
      mutable MaxHealth: int
      mutable Attack: int
      mutable Armor: int
      mutable Agility: int16 }

    static member Default =
        { Health = 10
          MaxHealth = 10
          Attack = 3
          Armor = 2
          Agility = int16 75 }

type IActor =
    abstract member Letter: string
    abstract member Position: Position with get, set
    abstract member Stats: Stats with get, set

type IMonster =
    inherit IActor
    abstract member Gold: int16
    abstract member Experience: int16
    abstract member Behavior: Behavior with get, set

and Behavior =
    | Basic // Just don't give a fuck about anything really
    | Greedy // If he sees player goes to a gold pile and hides it, then becomes mean
    | Mean // Attacks first
    | Flying // AHHHHHHHH WHERE DOES IT GO

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

module ActorScene =
    let Teleport actor x y =
        actor.Position.X <- x
        actor.Position.Y <- y

    let Move actor deltaX deltaY =
        Teleport actor (actor.Position.X + deltaX) (actor.Position.Y + deltaY)

    let MoveChecked check actor deltaX deltaY =
        let x' = actor.Position.X + deltaX
        let y' = actor.Position.Y + deltaY

        match check x' y' with
        | 0 ->
            Teleport actor x' y'
            0
        | a -> a
