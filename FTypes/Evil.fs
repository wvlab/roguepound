namespace FunctionalRoguePound

open FunctionalRoguePound.FUtility

module Evil =
    let CreateMonster letter behavior stats exp gold =
        let mutable position = { X = 0; Y = 0 }
        let mutable _behavior = behavior
        let mutable _stats = stats

        { new IMonster with
            member x.Letter = letter
            member x.Gold = gold
            member x.Experience = exp

            member x.Behavior
                with get () = _behavior
                and set v = _behavior <- v

            member x.Stats
                with get () = _stats
                and set v = _stats <- v

            member x.Position
                with get () = position
                and set v = position <- v }

    let CreateAquator stats =
        let changeStats stats = { stats with Attack = 0 }
        CreateMonster "A" Mean (changeStats stats)
