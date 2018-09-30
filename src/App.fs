module MonthlySavingsJuggler.App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fulma
open Fulma.Extensions

open MonthlySavingsJuggler.Accounts

// MODEL

type Stats12Mo =
  { totalDeposit: decimal
    totalBalance: decimal
    interestPaid: decimal }

  static member (+) (a, b) =
    { interestPaid = a.interestPaid + b.interestPaid
      totalBalance = a.totalBalance + b.totalBalance
      totalDeposit = a.totalDeposit + b.totalDeposit }

  static member Zero =
    { totalDeposit = 0m; totalBalance = 0m; interestPaid = 0m }

type AccountWithStats =
  { account: Account
    monthlyDeposit: decimal
    stats: Stats12Mo }

type Model =
  { monthlyBudget: decimal
    uninvestedBudget: decimal
    accounts: seq<AccountWithStats>
    unselectedAccounts: seq<Account>
    stats: Stats12Mo
    totalAer: decimal }

type Msg =
  | ChangeMonthlyBudget of decimal
  | PickAccounts
  | CalculateStats of seq<decimal * Account>


// STATE

let assignDeposits availableAccounts budget =
  let f (remainingAllowance: decimal) (account: Account) =
    if fst account.monthlyAllowance <= remainingAllowance then
      let accountBudget = min remainingAllowance (snd account.monthlyAllowance)
      let uninvestedBudget = remainingAllowance - accountBudget
      (accountBudget, account), uninvestedBudget
    else
      (0m, account), remainingAllowance

  availableAccounts
  |> Seq.sortByDescending (fun x -> x.aer, snd x.monthlyAllowance)
  |> Seq.mapFold f budget

let calculateStats accountWithDeposit =
  let monthlyDeposit, account = accountWithDeposit
  let totalDeposit = monthlyDeposit * 12m
  let interestPaid = totalDeposit * account.aer
  { account = account
    monthlyDeposit = monthlyDeposit
    stats =
      { totalDeposit = totalDeposit
        totalBalance = totalDeposit + interestPaid
        interestPaid = interestPaid } }

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
    unselectedAccounts = AvailableAccounts.List
    stats = Stats12Mo.Zero
    totalAer = 0m },
  Cmd.ofMsg (ChangeMonthlyBudget 500m)

let update (msg:Msg) (model:Model) =
  match msg with
  | ChangeMonthlyBudget monthlyBudget -> { model with monthlyBudget = monthlyBudget }, Cmd.ofMsg PickAccounts
  | PickAccounts ->
    let accountsWithDeposits, uninvestedBudget = assignDeposits AvailableAccounts.List model.monthlyBudget
    { model with uninvestedBudget = uninvestedBudget
                 unselectedAccounts = accountsWithDeposits |> Seq.filter (fun x -> fst x = 0m) |> Seq.map snd },
    Cmd.ofMsg (CalculateStats (accountsWithDeposits |> Seq.filter (fun x -> fst x > 0m)))
  | CalculateStats accountsWithDeposits ->
    let accounts = accountsWithDeposits |> Seq.map calculateStats
    let stats = accounts |> Seq.fold (fun stats account -> stats + account.stats) Stats12Mo.Zero
    // let stats = accounts |> Seq.sumBy (fun x -> x.stats) // todo: should work, but doesn't
    let totalAer = stats.interestPaid / stats.totalDeposit
    { model with accounts = accounts; stats = stats; totalAer = totalAer}, Cmd.none


// VIEW

let fmtCurrencyFrac (fraction: int) value : string =
  value?toLocaleString$("en-GB", createObj [ "style" ==> "currency"; "currency" ==> "GBP" ])

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
          str "The maximum monthly deposit while getting the highest AER of "
          strong [] [ str (sprintf "%.2f%%" (highestAer * 100m)) ]
          str " is currently "
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

let currentAccountInfo currentAccount =
  let requirements =
    if Seq.isEmpty currentAccount.requirements
    then [ p [ ClassName "is-size-7" ] [ str "No requirements for free current account." ] ]
    else [ p [ ClassName "is-size-7"] [ str "Requirements to get the current account for free:" ]
           ul [] [ for req in currentAccount.requirements -> li [ ClassName "is-size-7" ] [ str req ] ] ]

  [ Heading.h5 []
      [ a [ Href currentAccount.url ] [ str currentAccount.name ] ]
    Heading.h6 [ Heading.IsSubtitle ]
      [ str "required current account" ]
    div [] requirements ]

let accountCard account content =
  Card.card []
    [ Card.content []
        [ Media.media []
            [ Media.left []
                [ Image.image [ Image.Is64x64 ] [ img [ Src account.logoUrl ] ] ]
              Media.content []
                [ Heading.h4 [] [ a [ Href account.url ] [ str account.name ] ]
                  Heading.h5 [ Heading.IsSubtitle ] [ str account.bank ] ] ]
          Content.content [] content
          Level.level [ ]
            [ Level.item [ Level.Item.HasTextCentered ]
                [ div [ ]
                    [ Level.heading [ ] [ str "Min deposit" ]
                      Level.title [ ] [ str (sprintf "£%.0f" (fst account.monthlyAllowance)) ] ] ]
              Level.item [ Level.Item.HasTextCentered ]
                [ div [ ]
                    [ Level.heading [ ] [ str "Max deposit" ]
                      Level.title [ ] [ str (sprintf "£%.0f" (snd account.monthlyAllowance)) ] ] ]
              Level.item [ Level.Item.HasTextCentered ]
                [ div [ ]
                    [ Level.heading [ ] [ str "AER" ]
                      Level.title [ ] [ str (sprintf "%.1f%%" (account.aer * 100m)) ] ] ] ]
          Content.content [] (currentAccountInfo account.currentAccount)
        ]
    ]

let accountColumn account content =
  Column.column
    [ Column.Width (Screen.FullHD, Column.IsOneQuarter)
      Column.Width (Screen.Desktop, Column.IsOneThird)
      Column.Width (Screen.Tablet, Column.IsHalf)
      Column.Width (Screen.Mobile, Column.IsFull) ]
    [ accountCard account content ]

let selectedAccounts model dispatch =
  Container.container
    [ Container.IsFluid ]
    [ Heading.h3 [] [ str "Selected accounts" ]
      Columns.columns
        [ Columns.IsMultiline ]
        [ for account in model.accounts ->
          accountColumn account.account
            [ dl []
                [ dt [] [ str "Monthly investment" ]
                  dd [] [ str (fmtCurrency account.monthlyDeposit) ]
                  dt [] [ str "Deposited after 12 months" ]
                  dd [] [ str (fmtCurrency account.stats.totalDeposit) ]
                  dt [] [ str "Balance after 12 months" ]
                  dd [] [ str (fmtCurrency account.stats.totalBalance) ]
                  dt [] [ str "Interest paid after 12 months" ]
                  dd [] [ str (fmtCurrency account.stats.interestPaid) ] ]
            ]
          ]
    ]

let otherAccounts model dispatch =
  let accountsOrMessage =
    if Seq.isEmpty model.unselectedAccounts
    then p [] [ str "There are no more savings accounts available. You've maxed them all 🚀" ]
    else Columns.columns [ Columns.IsMultiline ] [ for account in model.unselectedAccounts -> accountColumn account [] ]
  Container.container
    [ Container.IsFluid ]
    [ Heading.h3 [] [ str "Other available accounts" ]
      accountsOrMessage ]

let view (model:Model) dispatch =
  div []
    [ header model dispatch
      Section.section []
       [ Columns.columns []
           [ Column.column [] [ controls model dispatch ]
             Column.column [] [ yearlyStats model dispatch ] ] ]
      Section.section []
        [ selectedAccounts model dispatch ]
      Section.section []
        [ otherAccounts model dispatch ]
    ]


// APP

Program.mkProgram init update view
// |> Program.withHMR
|> Program.withReact "monthly-savings-juggler"
|> Program.withConsoleTrace
|> Program.run
