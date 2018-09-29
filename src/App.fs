module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props

open App.Domain
open App.Accounts

// MODEL

type Model =
  { monthlyBudget: decimal
    uninvestedBudget: decimal
    accounts: seq<AccountWithStats>
    stats: Stats12Mo
    totalAer: decimal }

type Msg =
  | ChangeMonthlyBudget of decimal
  | PickAccounts
  | CalculateStats of seq<decimal * Account>


// STATE

let init() =
  let maxInvestment = AvailableAccounts.List |> Seq.fold (fun sum a -> sum + (snd a.monthlyAllowance)) 0m
  { monthlyBudget = 0m
    uninvestedBudget = 0m
    accounts = Seq.empty
    stats = Stats12Mo.zero
    totalAer = 0m },
  Cmd.ofMsg (ChangeMonthlyBudget maxInvestment)

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget x -> { model with monthlyBudget = x }, Cmd.ofMsg PickAccounts
  | PickAccounts ->
    let selectedAccounts, uninvestedBudget = pickAccounts AvailableAccounts.List model.monthlyBudget
    { model with uninvestedBudget = uninvestedBudget }, Cmd.ofMsg (CalculateStats selectedAccounts)
  | CalculateStats accountWithInvestment ->
    let accounts = accountWithInvestment |> Seq.map calculateStats
    let stats = accounts |> Seq.fold (fun stats account -> stats + account.stats) Stats12Mo.zero
    let totalAer = stats.interestPaid / stats.totalInvestment
    { model with accounts = accounts; stats = stats; totalAer = totalAer}, Cmd.none


// VIEW

let fmtCurrency value : string =
  value?toLocaleString$("en-GB", createObj [ "style" ==> "currency"; "currency" ==> "GBP"; "maximumFractionDigits" ==> 2 ])

let view (model:Model) dispatch =
  div
    []
    [ div
        []
        [ input [ OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ] ]
      div
        []
        [
          p [] [ str (sprintf "Monthly budget: %s" (fmtCurrency model.monthlyBudget)) ]
          p [] [ str (sprintf "Uninvested budget: %s" (fmtCurrency model.uninvestedBudget)) ]
          p [] [ str (sprintf "Investment after 12 months: %s" (fmtCurrency model.stats.totalInvestment)) ]
          p [] [ str (sprintf "Balance after 12 months: %s" (fmtCurrency model.stats.totalBalance)) ]
          p [] [ str (sprintf "Interest paid after 12 months:%s" (fmtCurrency model.stats.interestPaid)) ]
          p [] [ str (sprintf "Total AER %.2f%%" (model.totalAer * 100m)) ]
          ul
            []
            [ for account in model.accounts ->
              li
                []
                [ h3 [] [ str (sprintf "%s: %s (AER %.2f%%)" account.account.bank account.account.name (account.account.aer * 100m)) ]
                  p [] [ str (sprintf "Monthly investment: %s (of %s allowance)" (fmtCurrency account.monthlyInvestment) (fmtCurrency (snd account.account.monthlyAllowance))) ]
                  p [] [ str (sprintf "Investment after 12 months: %s" (fmtCurrency account.stats.totalInvestment)) ]
                  p [] [ str (sprintf "Balance after 12 months: %s" (fmtCurrency account.stats.totalBalance)) ]
                  p [] [ str (sprintf "Interest paid after 12 months: %s" (fmtCurrency account.stats.interestPaid)) ]
                ]
            ]
        ]
    ]


// APP

Program.mkProgram init update view
|> Program.withReact "monthly-savings-juggler"
|> Program.withConsoleTrace
|> Program.run
