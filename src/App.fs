module App

open Elmish
open Elmish.React
open Fable.Helpers.React
open Fable.Helpers.React.Props

// MODEL

type Account =
  { name: string
    monthlyAllowance: decimal
    interest: decimal }

type Model =
  { monthlyBudget: decimal }

type Msg =
  | ChangeMonthlyBudget of decimal


// STATE

let init() =
  { monthlyBudget = 0m }

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget x -> { model with monthlyBudget = x }

// VIEW (rendered with React)

let view (model:Model) dispatch =
  div []
    [ input [ OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ]
      div [] [ str (string model) ] ]


// APP
Program.mkSimple init update view
|> Program.withReact "elmish-app"
|> Program.withConsoleTrace
|> Program.run
