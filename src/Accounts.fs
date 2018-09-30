module MonthlySavingsJuggler.Accounts

type CurrentAccount =
  { name: string
    url: string
    aer: decimal
    requirements: seq<string> }

type Account =
  { name: string
    bank: string
    logoUrl: string
    monthlyAllowance: decimal * decimal
    aer: decimal
    url: string
    currentAccount: CurrentAccount }

type AvailableAccounts =
  static member List = [
    { name = "Regular Saver Account"
      bank = "First Direct"
      logoUrl = "https://logo.clearbit.com/firstdirect.com"
      monthlyAllowance = 25m, 300m
      aer = 0.05m
      url = "https://www1.firstdirect.com/1/2/savings-and-investments/savings/regular-saver-account"
      currentAccount =
        { name = "1st Account"
          url = "https://www1.firstdirect.com/1/2/banking/current-account"
          aer = 0m
          requirements =
            [ "paying at least £1,000 into the account every month"
              "or maintaining an average monthly balance of £1,000"
              "or also having a mortgage, credit card, Personal Loan, savings (except Regular Saver), First Directory or Home insurance with First Direct" ] } }

    { name = "Regular Saver Account"
      bank = "M&S Bank"
      logoUrl = "https://logo.clearbit.com/marksandspencer.com"
      monthlyAllowance = 25m, 250m
      aer = 0.05m
      url = "https://bank.marksandspencer.com/save-invest/monthly-saver/"
      currentAccount =
        { name = "M&S Current Account"
          url = "https://bank.marksandspencer.com/current-accounts/mands-current-account/"
          aer = 0m
          requirements = Seq.empty } }

    { name = "Regular Saver Account"
      bank = "Nationwide"
      logoUrl = "https://logo.clearbit.com/nationwide.co.uk"
      monthlyAllowance = 1m, 250m
      aer = 0.05m
      url = "https://www.nationwide.co.uk/products/savings/flex-regular-online-saver/features-and-benefits"
      currentAccount =
        { name = "FlexDirect"
          url = "https://www.nationwide.co.uk/products/current-accounts/flexdirect/features-and-benefits"
          aer = 0.01m
          requirements = Seq.empty } }

    { name = "Regular eSaver"
      bank = "Santander"
      logoUrl = "https://logo.clearbit.com/santander.co.uk"
      monthlyAllowance = 1m, 200m
      aer = 0.05m
      url = "https://www.santander.co.uk/uk/savings/regular-esaver"
      currentAccount =
        { name = "1|2|3 Lite Current Account"
          url = "https://www.santander.co.uk/uk/current-accounts/123-lite-current-account"
          aer = 0.01m
          requirements =
            [ "there is a £1 monthly account fee that will be automatically taken from your account each month"
              "pay at least £500 into your account each month - any payments between Santander personal accounts you're named on won't count towards this"
              "have at least two active Direct Debits - you'll receive cashback on any qualifying household bills you pay by Direct Debit"
              "log onto Online or Mobile Banking at least once in every three months"] } }

    { name = "Regular Saver"
      bank = "HSBC"
      logoUrl = "https://logo.clearbit.com/hsbc.com"
      monthlyAllowance = 25m, 250m
      aer = 0.05m
      url = "https://www.hsbc.co.uk/savings/products/regular-saver"
      currentAccount =
        { name = "HSBC Advance"
          url = "https://www.hsbc.co.uk/current-accounts/products/advance"
          aer = 0m
          requirements = Seq.empty } }

    { name = "Club Lloyds Monthly Saver"
      bank = "Lloyds"
      logoUrl = "https://logo.clearbit.com/lloydsbank.com"
      monthlyAllowance = 25m, 400m
      aer = 0.03m
      url = "https://www.lloydsbank.com/savings/club-lloyds-monthly-saver.asp"
      currentAccount =
        { name = "Club Lloyds Current Account"
          url = "https://www.lloydsbank.com/current-accounts/all-accounts/club-lloyds.asp"
          aer = 0.015m
          requirements =
            [ "if in any month you pay less than £1,500 into your account, you'll have to pay the £3 monthly account fee"
              "if you pay out two separate Direct Debits each month, you'll earn 1.5% AER variable credit interest (paid monthly) on balances between £1 and £5,000"] } }

    { name = "Monthly Saver"
      bank = "Lloyds"
      logoUrl = "https://logo.clearbit.com/lloydsbank.com"
      monthlyAllowance = 25m, 250m
      aer = 0.025m
      url = "https://www.lloydsbank.com/savings/monthly-saver.asp"
      currentAccount =
        { name = "Any Lloyds current account"
          url = "https://www.lloydsbank.com/current-accounts.asp"
          aer = 0m
          requirements = Seq.empty } }
  ]