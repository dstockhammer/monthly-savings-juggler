var isProduction = !process.argv.find(v => v.indexOf("webpack-dev-server") !== -1);

var path = require("path");
var HtmlWebpackPlugin = require("html-webpack-plugin");
var CopyWebpackPlugin = require("copy-webpack-plugin");
// var MiniCssExtractPlugin = require("mini-css-extract-plugin");

var commonPlugins = [
  new HtmlWebpackPlugin({
    filename: "index.html",
    template: "./src/index.html"
  })
];

module.exports = {
  mode: isProduction ? "production" : "development",
  devtool: isProduction ? "source-map" : "eval-source-map",
  optimization: {
    // Split the code coming from npm packages into a different file.
    // 3rd party dependencies change less often, let the browser cache them.
    splitChunks: {
      cacheGroups: {
        commons: {
          test: /node_modules/,
          name: "vendors",
          chunks: "all"
        }
      }
    },
  },
  entry: {
    app: ["./src/MonthlySavingsJuggler.fsproj"],
    style: ["./src/scss/main.scss"]
  },
  output: {
    path: path.join(__dirname, "./dist"),
    filename: isProduction ? "[name].[hash].js" : "[name].js",
  },
  plugins: isProduction ?
    commonPlugins.concat([
      // new MiniCssExtractPlugin({ filename: "style.css" }),
      new CopyWebpackPlugin([{ from: "./assets" }]),
    ])
    : commonPlugins.concat([
      // new webpack.HotModuleReplacementPlugin(),
    ]),
  resolve: {
    // See https://github.com/fable-compiler/Fable/issues/1490
    symlinks: false
  },
  devServer: {
    contentBase: "./assets",
    port: 8080,
  },
  module: {
    rules: [
      {
        test: /\.fs(x|proj)?$/,
        use: "fable-loader"
      },
      {
        test: /\.(sass|scss|css)$/,
        use: [
          "style-loader",
          "css-loader",
          "sass-loader",
        ],
      },
      {
        test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
        use: ["file-loader"]
      }
    ]
  }
}