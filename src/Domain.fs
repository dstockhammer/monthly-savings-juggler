module App.Domain

type Account =
  { name: string
    bank: string
    monthlyAllowance: decimal * decimal
    aer: decimal
    url: string
    requirements: string }

type Stats12Mo =
  { totalDeposit: decimal
    totalBalance: decimal
    interestPaid: decimal }

  static member (+) (a, b) =
    { interestPaid = a.interestPaid + b.interestPaid
      totalBalance = a.totalBalance + b.totalBalance
      totalDeposit = a.totalDeposit + b.totalDeposit }

  static member zero =
    { totalDeposit = 0m; totalBalance = 0m; interestPaid = 0m }

type AccountWithStats =
  { account: Account
    monthlyDeposit: decimal
    stats: Stats12Mo }


let pickAccounts availableAccounts budget =
  let f (remainingAllowance: decimal) (account: Account) =
    let accountBudget = min remainingAllowance (snd account.monthlyAllowance)
    let uninvestedBudget = remainingAllowance - accountBudget
    (accountBudget, account), uninvestedBudget

  availableAccounts
  |> Seq.sortByDescending (fun x -> x.aer, (snd x.monthlyAllowance))
  |> Seq.mapFold f budget
  |> fun x -> Seq.filter (fun x -> fst x > 0m) (fst x), snd x

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
