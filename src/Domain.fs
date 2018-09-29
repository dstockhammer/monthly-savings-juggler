module App.Domain

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


let pickAccounts availableAccounts budget =
  let f (remainingAllowance: decimal) (account: Account) =
    let accountBudget = min remainingAllowance (snd account.monthlyAllowance)
    let uninvestedBudget = remainingAllowance - accountBudget
    (accountBudget, account), uninvestedBudget

  availableAccounts
  |> Seq.sortByDescending (fun x -> x.aer)
  |> Seq.mapFold f budget
  |> fun x -> Seq.filter (fun x -> fst x > 0m) (fst x), snd x

let calculateStats accountWithInvestment =
  let monthlyInvestment, account = accountWithInvestment
  let totalInvestment = monthlyInvestment * 12m
  let interestPaid = totalInvestment * account.aer
  { account = account
    monthlyInvestment = monthlyInvestment
    stats =
      { totalInvestment = totalInvestment
        totalBalance = totalInvestment + interestPaid
        interestPaid = interestPaid } }