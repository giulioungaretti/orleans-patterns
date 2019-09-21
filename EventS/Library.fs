namespace EventS

open Orleans.Patterns.EventSourcing


module Real =
    let inline notNull value = if (obj.ReferenceEquals(value, null))  then None else Some value

    type Number =
        | Real of float
        member this.Add(n : Number) =
            match (this, n) with
            | (Real n1, Real n2) -> Real(n1 + n2)

    let ProcessEvent(state : struct (System.Guid * System.DateTimeOffset * Number ),
                     curr : BusinessEvent) =

        // NOTE: this can be null but really it should be Result type, this would allow us to try different json converters!
        let current = curr.GetValue<Number>()

        let struct (seedId, seedTimestamp, folded) = state

        let (id, timestamp) =
            if curr.EventTimestamp > seedTimestamp then
                (curr.EventIdentifier, curr.EventTimestamp)
            else (seedId, seedTimestamp)
        match (notNull folded) with 
        | Some f ->
            struct (id, timestamp, f.Add(current))
        | None ->
            struct (id, timestamp, current)

    let InitializeSeed(seed : Number ) =
        match (notNull seed) with 
        | Some s ->
            System.Func<struct (System.Guid * System.DateTimeOffset * Number)>
                (fun () ->
                struct (System.Guid.Empty, System.DateTimeOffset.MinValue, s))
        | None -> 
            System.Func<struct (System.Guid * System.DateTimeOffset * Number)>
                (fun () ->
                struct (System.Guid.Empty, System.DateTimeOffset.MinValue, Real 0.0))

module Complex =

    let inline notNull value = if (obj.ReferenceEquals(value, null))  then None else Some value

    type Number =
        | Real of float
        | Complex of (float * float)

        member this.Add(n : Number) =
            match (this, n) with
            | (Real n1, Real n2) -> Real(n1 + n2)
            // sacrifice real maths for sensible ops 
            | (Real n1, Complex(real, img)) -> Complex(n1 + real, img)
            | (Complex(real, img), Real n1) -> Complex(n1 + real, img)
            //
            | (Complex(real, img), Complex(real1, img1)) ->
                Complex(real + real1, img + img1)

        member this.RealCompoment =
            match this with
            | Real n ->
                n
            | Complex (n, i) ->
                n
        member this.ImaginaryComponent =
            match this with
            | Real _ ->
                0.0
            | Complex (n, i) ->
                i

    let ProcessEvent(state : struct (System.Guid * System.DateTimeOffset * Number ),
                     curr : BusinessEvent) =
        // NOTE: this can be null but really it should be Result type, this would allow us to try different json converters!
        let current = curr.GetValue<Number>()
        let struct (seedId, seedTimestamp, folded) = state

        let (id, timestamp) =
            if curr.EventTimestamp > seedTimestamp then
                (curr.EventIdentifier, curr.EventTimestamp)
            else (seedId, seedTimestamp)

        match (notNull folded) with 
        | Some f ->
            struct (id, timestamp, f.Add(current))
        | None ->
            struct (id, timestamp, current)

    let InitializeSeed(seed : Number ) =
        match (notNull seed) with 
        | Some s ->
            System.Func<struct (System.Guid * System.DateTimeOffset * Number)>
                (fun () ->
                struct (System.Guid.Empty, System.DateTimeOffset.MinValue, s))
        | None -> 
            System.Func<struct (System.Guid * System.DateTimeOffset * Number)>
                (fun () ->
                struct (System.Guid.Empty, System.DateTimeOffset.MinValue, Complex (0.0, 0.00)))