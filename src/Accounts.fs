module App.Accounts

open App.Domain

type AvailableAccounts =
  static member List = [
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