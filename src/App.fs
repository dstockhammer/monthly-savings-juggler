module App.View

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fulma
open Fulma.Extensions

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

let highestAer =
  AvailableAccounts.List
  |> Seq.maxBy (fun x -> x.aer)
  |> fun x -> x.aer

let maxDeposit =
  AvailableAccounts.List
  |> Seq.sumBy (fun x -> snd x.monthlyAllowance)

let maxDepositForHighestAer =
  AvailableAccounts.List
  |> Seq.filter (fun x -> x.aer = highestAer)
  |> Seq.sumBy (fun x -> snd x.monthlyAllowance)

let init() =
  { monthlyBudget = 0m
    uninvestedBudget = 0m
    accounts = Seq.empty
    stats = Stats12Mo.Zero
    totalAer = 0m },
  Cmd.ofMsg (ChangeMonthlyBudget 500m)

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget monthlyBudget -> { model with monthlyBudget = monthlyBudget }, Cmd.ofMsg PickAccounts
  | PickAccounts ->
    let selectedAccounts, uninvestedBudget = pickAccounts AvailableAccounts.List model.monthlyBudget
    { model with uninvestedBudget = uninvestedBudget }, Cmd.ofMsg (CalculateStats selectedAccounts)
  | CalculateStats accountWithInvestment ->
    let accounts = accountWithInvestment |> Seq.map calculateStats
    let stats = accounts |> Seq.fold (fun stats account -> stats + account.stats) Stats12Mo.Zero
    // let stats = accounts |> Seq.sumBy (fun x -> x.stats) // todo: should work, but doesn't
    let totalAer = stats.interestPaid / stats.totalDeposit
    { model with accounts = accounts; stats = stats; totalAer = totalAer}, Cmd.none


// VIEW

let fmtCurrency value =
  value?toLocaleString$("en-GB", createObj [ "style" ==> "currency"; "currency" ==> "GBP"; "maximumFractionDigits" ==> 2 ])

let header model dispatch =
  Hero.hero
    [ Hero.Color IsPrimary ]
    [ Hero.body []
        [ Container.container
            [ Container.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
            [ Heading.h1 []
                [ str "Monthly Savings Juggler" ]
              Heading.h3 [ Heading.IsSubtitle ]
                [ str "Optimise UK monthly savings accounts" ] ] ] ]


let controls model dispatch =
  Container.container
    [ Container.IsFluid ]
    [ Heading.h4 [] [ str "Monthly budget" ]
      div []
        [ str "Move the slider to ajust how much money you want to deposit "
          strong [] [ str "each month" ]
          str ". "
          str "The maximum monthly deposit across all accounts is currently "
          Button.span
            [ Button.OnClick (fun _ -> maxDeposit |> ChangeMonthlyBudget |> dispatch)
              Button.Size IsSmall ]
            [ str (fmtCurrency maxDeposit) ]
          str ". "
          str "The breakpoint for getting the highest AER of "
          strong [] [ str (sprintf "%.2f%%" (highestAer * 100m)) ]
          str " is "
          Button.span
            [ Button.OnClick (fun _ -> maxDepositForHighestAer |> ChangeMonthlyBudget |> dispatch)
              Button.Size IsSmall ]
            [ str (fmtCurrency maxDepositForHighestAer) ]
          str ". " ]
      div []
        [ Slider.slider [ Slider.IsFullWidth
                          Slider.Step 1.0
                          Slider.Min 1.0
                          Slider.Max (float maxDeposit)
                          Slider.Value (float model.monthlyBudget)
                          Slider.OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ] ]
    ]


let yearlyStats model dispatch =
  Container.container
    [ Container.IsFluid ]
    [ Heading.h4 [] [ str "Balance after 12 months" ]
      dl []
        [ dt [] [ str "Monthly budget" ]
          dd [] [ str (fmtCurrency model.monthlyBudget) ]
          dt [] [ str "Uninvested budget" ]
          dd [] [ str (fmtCurrency model.uninvestedBudget) ]
          dt [] [ str "Deposited after 12 months" ]
          dd [] [ str (fmtCurrency model.stats.totalDeposit) ]
          dt [] [ str "Balance after 12 months" ]
          dd [] [ str (fmtCurrency model.stats.totalBalance) ]
          dt [] [ str "Interest paid after 12 months" ]
          dd [] [ str (fmtCurrency model.stats.interestPaid) ]
          dt [] [ str "Total AER" ]
          dd [] [ str (sprintf "%.2f%%" (model.totalAer * 100m)) ] ]
      br []
    ]

let selectedAccounts model dispatch =
  Container.container
    [ Container.IsFluid ]
    [ Heading.h3 [] [ str "Selected accounts" ]
      Columns.columns
        [ Columns.IsMultiline ]
        [ for account in model.accounts ->
          Column.column
            [ Column.Width (Screen.FullHD, Column.IsOneQuarter)
              Column.Width (Screen.Desktop, Column.IsOneThird)
              Column.Width (Screen.Tablet, Column.IsHalf)
              Column.Width (Screen.Mobile, Column.IsFull) ]
            [ Card.card []
                [ Card.header []
                    [ Card.Header.title
                        [ Card.Header.Title.IsCentered ]
                        [ str (sprintf "%s: %s (AER %.2f%%)" account.account.bank account.account.name (account.account.aer * 100m)) ] ]
                  Card.content []
                    [ Content.content []
                        [ dl []
                            [ dt [] [ str "Monthly investment" ]
                              dd [] [ str (fmtCurrency account.monthlyDeposit) ]
                              dt [] [ str "Limits" ]
                              dd [] [ str (fmtCurrency (fst account.account.monthlyAllowance))
                                      str " - "
                                      str (fmtCurrency (snd account.account.monthlyAllowance)) ]
                              dt [] [ str "Deposited after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.totalDeposit) ]
                              dt [] [ str "Balance after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.totalBalance) ]
                              dt [] [ str "Interest paid after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.interestPaid) ] ] ] ]
                ]
            ]
          ]
    ]

let view (model:Model) dispatch =
  div []
    [ header model dispatch
      Section.section []
       [ Columns.columns []
           [ Column.column [] [ controls model dispatch ]
             Column.column [] [ yearlyStats model dispatch ] ] ]
      Section.section []
        [ selectedAccounts model dispatch ]
    ]


// APP

Program.mkProgram init update view
// |> Program.withHMR
|> Program.withReact "monthly-savings-juggler"
|> Program.withConsoleTrace
|> Program.run
