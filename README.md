# Monthly Savings Juggler

Many (most?) banks in the UK offer “regular” monthly savings accounts that are available to customers who also hold a current account with that bank. The interest rates are decent essentially risk free (see FCA deposit and savings protection). There is a monthly allowance that varies between banks, but it’s usually around £300. In order to save more than that, you have to set up multiple accounts with the different banks and set up standing orders in a way that you a) deposit funds into the correct savings accounts on the 1st of each month and b) ensure that you meet the eligibility criteria for the respective current account.

This calculater helps you make the most out of your monthly savings budget. It allocates your budget into different acconts, prioritising account with the highest interest and minimising the total number of accounts required. You still have to ensure that you satisfy the requirements for the free current account.

In a future version, the calculator may help you with those requirements as well and may spit out full instructions to create all standing orders and/or direct debits. For now, numbers and stats is all you get.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 2.0 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)

## Building and running the app for local development

* Install JS dependencies: `npm install`
* Install F# dependencies: `dotnet restore src`
* Move to `src` directory to start Fable and Webpack dev server: `dotnet fable webpack-dev-server`
* After the first compilation is finished, in your browser open: http://localhost:8080/

Any modification you do to the F# code will be reflected in the web page after saving.

## Building for prod
* In `src` directory: `dotnet fable webpack-cli`
* Copy contents from `/dist` folder to `gh-pages` branch for GitHub pages

## Project structure

### npm

JS dependencies are declared in `package.json`, while `package-lock.json` is a lock file automatically generated.

### Webpack

[Webpack](https://webpack.js.org) is a JS bundler with extensions, like a static dev server that enables hot reloading on code changes. Fable interacts with Webpack through the `fable-loader`. Configuration for Webpack is defined in the `webpack.config.js` file.

### F#

The sample only contains two F# files: the project (.fsproj) and a source file (.fs) in the `src` folder.

### Web assets

The `index.html` file and other assets like an icon can be found in the `public` folder.
