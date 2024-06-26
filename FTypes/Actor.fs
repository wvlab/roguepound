namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility
open Result

type Stats =
    { mutable Health: int
      mutable MaxHealth: int
      mutable Attack: int
      mutable Armor: int16
      mutable Agility: int16 }

    static member Default =
        { Health = 10
          MaxHealth = 10
          Attack = 3
          Armor = 2s
          Agility = 2s }

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
    member val Level = 1s with get, set
    member val Experience = 0 with get, set
    member val ExperienceCap = 7 with get, set

    interface IActor with
        member val Letter = "@"
        member val Position: Position = { X = 0; Y = 0 } with get, set

        member val Stats: Stats =
            { Stats.Default with
                MaxHealth = 17
                Health = 17 } with get, set

    member public this.Letter = (this :> IActor).Letter
    member public this.Position = (this :> IActor).Position
    member public this.Stats = (this :> IActor).Stats

module ActorScene =
    let Teleport (actor: IActor) x y =
        actor.Position.X <- x
        actor.Position.Y <- y

    let Move actor deltaX deltaY =
        Teleport actor (actor.Position.X + deltaX) (actor.Position.Y + deltaY)

    let MoveChecked check (actor: IActor) deltaX deltaY =
        let x' = actor.Position.X + deltaX
        let y' = actor.Position.Y + deltaY

        match check x' y' with
        | 0 ->
            Teleport actor x' y'
            0
        | a -> a

    let Attack (rnd: System.Random) (lead: IActor) (victim: IActor) =
        let checkStat stat message =
            if stat > int16 (rnd.Next(0, 101)) then
                Error message
            else
                Ok()

        let checkArmor () =
            checkStat victim.Stats.Armor "Armor blocked your attack"

        let checkAgility () =
            checkStat victim.Stats.Agility "Opponent evaded your attack"

        let attack () =
            victim.Stats.Health <- (victim.Stats.Health - lead.Stats.Attack)

        match (checkArmor >> bind checkAgility) () with
        | Ok() -> attack ()
        | Error _ -> ()
