module App.View

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
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

let maxInvestment =
  AvailableAccounts.List
  |> Seq.fold (fun sum a -> sum + (snd a.monthlyAllowance)) 0m

let init() =
  { monthlyBudget = 0m
    uninvestedBudget = 0m
    accounts = Seq.empty
    stats = Stats12Mo.zero
    totalAer = 0m },
  Cmd.ofMsg (ChangeMonthlyBudget (maxInvestment / 2m))

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget monthlyBudget -> { model with monthlyBudget = monthlyBudget }, Cmd.ofMsg PickAccounts
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
  div []
    [
      Hero.hero [ Hero.Color IsPrimary ]
        [ Hero.body []
            [ Container.container
                [ Container.Modifiers [ Modifier.TextAlignment(Screen.All, TextAlignment.Centered) ] ]
                [ Heading.h1 []
                    [ str "Monthly Savings Juggler" ]
                  Heading.h2 [ Heading.IsSubtitle ]
                    [ str "Subtitle" ] ] ] ]

      Section.section []
        [ Container.container [ Container.IsFluid ]
            [ Heading.h3 [] [ str "Monthly Budget" ]
              div []
                [ str "Lorem ipsum dolor sit amet, consectetur adipiscing elit.
                      Nulla accumsan, metus ultrices eleifend gravida, nulla nunc varius lectus
                      , nec rutrum justo nibh eu lectus. Ut vulputate semper dui. Fusce erat odio
                      , sollicitudin vel erat vel, interdum mattis neque." ]
              div []
                [ Slider.slider [ Slider.IsFullWidth
                                  Slider.Step 1.0
                                  Slider.Min 1.0
                                  Slider.Max (float maxInvestment)
                                  Slider.DefaultValue (float model.monthlyBudget)
                                  Slider.OnChange (fun ev -> decimal(ev.Value) |> ChangeMonthlyBudget |> dispatch) ] ]
            ]
        ]

      Section.section []
        [ Container.container [ Container.IsFluid ]
            [ Heading.h3 [] [ str "Savings Accounts" ]
              dl
                []
                [ dt [] [ str "Monthly budget" ]
                  dd [] [ str (fmtCurrency model.monthlyBudget) ]
                  dt [] [ str "Investment after 12 months" ]
                  dd [] [ str (fmtCurrency model.stats.totalInvestment) ]
                  dt [] [ str "Balance after 12 months" ]
                  dd [] [ str (fmtCurrency model.stats.totalBalance) ]
                  dt [] [ str "Interest paid after 12 months" ]
                  dd [] [ str (fmtCurrency model.stats.interestPaid) ]
                  dt [] [ str "Total AER" ]
                  dd [] [ str (sprintf "%.2f%%" (model.totalAer * 100m)) ] ]
              br []
            ]
          Container.container [ Container.IsFluid ]
            [ for account in model.accounts ->
              Card.card []
                [ Card.header []
                    [ Card.Header.title [ Card.Header.Title.IsCentered ]
                        [ str (sprintf "%s: %s (AER %.2f%%)" account.account.bank account.account.name (account.account.aer * 100m)) ] ]
                  Card.content []
                    [ Content.content []
                           [ dl []
                            [ dt [] [ str "Monthly investment" ]
                              dd [] [ str (fmtCurrency account.monthlyInvestment) ]
                              dt [] [ str "Investment after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.totalInvestment) ]
                              dt [] [ str "Balance after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.totalBalance) ]
                              dt [] [ str "Interest paid after 12 months" ]
                              dd [] [ str (fmtCurrency account.stats.interestPaid) ] ] ] ]
                ]
            ]
        ]
      ]


// APP

Program.mkProgram init update view
// |> Program.withHMR
|> Program.withReact "monthly-savings-juggler"
|> Program.withConsoleTrace
|> Program.run
