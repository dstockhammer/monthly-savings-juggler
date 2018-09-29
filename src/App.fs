module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.Helpers.React
open Fable.Helpers.React.Props


// MODEL

type Account =
  { name: string
    bank: string
    monthlyAllowance: decimal * decimal
    aer: decimal
    url: string
    requirements: string }

type Stats12Mo =
  { totalInvestment: decimal
    totalBalance: decimal
    interestPaid: decimal }
  static member (+) (a, b) =
    { interestPaid = a.interestPaid + b.interestPaid
      totalBalance = a.totalBalance + b.totalBalance
      totalInvestment = a.totalInvestment + b.totalInvestment }
  static member zero =
    { totalInvestment = 0m; totalBalance = 0m; interestPaid = 0m }

type AccountWithStats =
  { account: Account
    monthlyInvestment: decimal
    stats: Stats12Mo }

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

let availableAccounts = [
  { name = "Regular Saver Account"
    bank = "First Direct"
    monthlyAllowance = 25m, 300m
    aer = 0.05m
    url = "https://www1.firstdirect.com/1/2/savings-and-investments/savings/regular-saver-account"
    requirements = "
    Current account: first direct 1st Account, AER 0%, https://www1.firstdirect.com/1/2/banking/current-account
    - paying at least £1,000 into the account every month
    - or maintaining an average monthly balance of £1,000
    - or also having a mortgage, credit card, Personal Loan, savings (except Regular Saver), First Directory or Home insurance with us." };

  { name = "Regular Saver Account"
    bank = "M&S Bank"
    monthlyAllowance = 25m, 250m
    aer = 0.05m
    url = "https://bank.marksandspencer.com/save-invest/monthly-saver"
    requirements = "
    Current account: M&S Current Account, AER 0%, https://bank.marksandspencer.com/current-accounts/mands-current-account
    - no requirements" };

  { name = "Regular Saver Account"
    bank = "Nationwide"
    monthlyAllowance = 1m, 250m
    aer = 0.05m
    url = "https://www.nationwide.co.uk/products/savings/flex-regular-online-saver/features-and-benefits"
    requirements = "
    Current account: FlexDirect, AER 1%, https://www.nationwide.co.uk/products/current-accounts/flexdirect/features-and-benefits
    - no requirements" };

  { name = "Regular eSaver"
    bank = "Santander"
    monthlyAllowance = 1m, 200m
    aer = 0.05m
    url = "https://www.santander.co.uk/uk/savings/regular-esaver"
    requirements = "
    Current account: 123 Lite Current Account, AER 1%, https://www.santander.co.uk/uk/current-accounts/123-lite-current-account
    There is a £1 monthly account fee that will be automatically taken from your account each month.
    What you need to do to earn cashback
    - Pay at least £500 into your account each month - any payments between Santander personal accounts you're named on won't count towards this
    - Have at least two active Direct Debits - you'll receive cashback on any qualifying household bills you pay by Direct Debit
    - Log onto Online or Mobile Banking at least once in every three months" };

  { name = "Regular Saver"
    bank = "HSBC"
    monthlyAllowance = 25m, 250m
    aer = 0.05m
    url = "https://www.hsbc.co.uk/savings/products/regular-saver"
    requirements = "
    Current account: HSBC Advance, AER 0%, https://www.hsbc.co.uk/current-accounts/products/advance
    - no requirements" };

  { name = "Club Lloyds Monthly Saver"
    bank = "Lloyds"
    monthlyAllowance = 25m, 400m
    aer = 0.03m
    url = "https://www.lloydsbank.com/savings/club-lloyds-monthly-saver.asp"
    requirements = "
    Current account: Club Lloyds Current Account, AER 1.5%, https://www.lloydsbank.com/current-accounts/all-accounts/club-lloyds.asp
    - pay in 1400£
    - pay out two separate direct debits each month" };

  { name = "Monthly Saver"
    bank = "Lloyds"
    monthlyAllowance = 25m, 250m
    aer = 0.025m
    url = "https://www.lloydsbank.com/savings/monthly-saver.asp"
    requirements = "
    Any lloyds account, 0% AER, no requirements" };
]


// DOMAIN LOGIC

let pickAccounts budget =
  let f (remainingAllowance: decimal) (account: Account) =
    let accountBudget = min remainingAllowance (snd account.monthlyAllowance)
    let uninvestedBudget = remainingAllowance - accountBudget
    (accountBudget, account), uninvestedBudget

  availableAccounts
  |> Seq.sortByDescending (fun x -> x.aer)
  |> Seq.mapFold f budget
  |> fun x -> Seq.filter (fun x -> fst x > 0m) (fst x), snd x

let calculateStats (accountWithInvestment: decimal * Account) =
  let monthlyInvestment, account = accountWithInvestment
  let totalInvestment = monthlyInvestment * 12m
  let interestPaid = totalInvestment * account.aer
  { account = account
    monthlyInvestment = monthlyInvestment
    stats =
      { totalInvestment = totalInvestment
        totalBalance = totalInvestment + interestPaid
        interestPaid = interestPaid } }


// STATE

let init() =
  let maxInvestment = availableAccounts |> Seq.fold (fun sum a -> sum + (snd a.monthlyAllowance)) 0m
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
    let selectedAccounts, uninvestedBudget = pickAccounts model.monthlyBudget
    { model with uninvestedBudget = uninvestedBudget }, Cmd.ofMsg (CalculateStats selectedAccounts)
  | CalculateStats accountWithInvestment ->
    let accounts = accountWithInvestment |> Seq.map calculateStats
    let stats = accounts |> Seq.fold (fun stats account -> stats + account.stats) Stats12Mo.zero
    let totalAer = stats.interestPaid / stats.totalInvestment
    { model with accounts = accounts; stats = stats; totalAer = totalAer}, Cmd.none


// VIEW (rendered with React)

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
