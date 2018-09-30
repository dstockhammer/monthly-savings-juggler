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

let maxDeposit =
  AvailableAccounts.List
  |> Seq.fold (fun sum a -> sum + (snd a.monthlyAllowance)) 0m

let init() =
  { monthlyBudget = 0m
    uninvestedBudget = 0m
    accounts = Seq.empty
    stats = Stats12Mo.zero
    totalAer = 0m },
  Cmd.ofMsg (ChangeMonthlyBudget (maxDeposit / 2m))

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget monthlyBudget -> { model with monthlyBudget = monthlyBudget }, Cmd.ofMsg PickAccounts
  | PickAccounts ->
    let selectedAccounts, uninvestedBudget = pickAccounts AvailableAccounts.List model.monthlyBudget
    { model with uninvestedBudget = uninvestedBudget }, Cmd.ofMsg (CalculateStats selectedAccounts)
  | CalculateStats accountWithInvestment ->
    let accounts = accountWithInvestment |> Seq.map calculateStats
    let stats = accounts |> Seq.fold (fun stats account -> stats + account.stats) Stats12Mo.zero
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
    [ //Heading.h3 [] [ str "Monthly budget" ]
      div []
        [ str "Lorem ipsum dolor sit amet, consectetur adipiscing elit.
              Nulla accumsan, metus ultrices eleifend gravida, nulla nunc varius lectus
              , nec rutrum justo nibh eu lectus. Ut vulputate semper dui. Fusce erat odio
              , sollicitudin vel erat vel, interdum mattis neque." ]
      div []
        [ Slider.slider [ Slider.IsFullWidth
                          Slider.Step 1.0
                          Slider.Min 1.0
                          Slider.Max (float maxDeposit)
                          Slider.DefaultValue (float model.monthlyBudget)
                          Slider.OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ] ]
    ]


let yearlyStats model dispatch =
  Container.container
    [ Container.IsFluid ]
    [ //Heading.h3 [] [ str "Balance after 12 months" ]
      dl []
        [ dt [] [ str "Monthly budget" ]
          dd [] [ str (fmtCurrency model.monthlyBudget) ]
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
