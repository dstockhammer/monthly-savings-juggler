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
  { monthlyBudget: decimal
    uninvestedBudget: decimal
    selectedAccounts: seq<decimal * Account>
 }

type Msg =
  | ChangeMonthlyBudget of decimal
  | PickAccounts

let availableAccounts = [
  { name = "Natwest Monthly"
    monthlyAllowance = 300m
    interest = 0.02m };
  { name = "Club Lloyds Monthly Saver"
    monthlyAllowance = 400m
    interest = 0.03m };
  { name = "EasySaver"
    monthlyAllowance = 500m
    interest = 0.015m };
]


// DOMAIN LOGIC

let pickAccounts budget =
  let f (remainingAllowance: decimal) (account: Account) =
    let accountBudget = min remainingAllowance account.monthlyAllowance
    let uninvestedBudget = remainingAllowance - accountBudget
    (accountBudget, account), uninvestedBudget

  availableAccounts
  |> Seq.sortByDescending (fun x -> x.interest)
  |> Seq.mapFold f budget
  |> fun x -> Seq.filter (fun x -> fst x > 0m) (fst x), snd x


// STATE

let init() =
  { monthlyBudget = 0m
    uninvestedBudget = 0m
    selectedAccounts = Seq.empty },
  Cmd.none

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget x -> { model with monthlyBudget = x }, Cmd.ofMsg PickAccounts
  | PickAccounts ->
    let selectedAccounts, uninvestedBudget = pickAccounts model.monthlyBudget
    { model with selectedAccounts = selectedAccounts
                 uninvestedBudget = uninvestedBudget },
    Cmd.none


// VIEW (rendered with React)

let view (model:Model) dispatch =
  div
    []
    [ div
        []
        [ input [ OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ] ]
      div
        []
        [
          p [] [ str (sprintf "Monthly budget: %M" model.monthlyBudget) ]
          p [] [ str (sprintf "Uninvested budget: %M" model.uninvestedBudget) ]
          ul
            []
            [ for account in model.selectedAccounts ->
              li
                []
                [ h3 [] [ str (sprintf "%s (APR %M)" (snd account).name (snd account).interest) ]
                  p [] [ str (sprintf "Monthly investment: £%M (of £%M allowance)" (fst account) (snd account).monthlyAllowance) ] ]
            ]
        ]
    ]


// APP

Program.mkProgram init update view
|> Program.withReact "monthly-savings-juggler"
|> Program.withConsoleTrace
|> Program.run
