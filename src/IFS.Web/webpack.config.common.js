/// <binding />
const path = require('path');
const webpack = require('webpack');
const targetDir = path.resolve(__dirname, 'wwwroot/build');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const globals = new webpack.ProvidePlugin({
    $: 'jquery',
    jQuery: 'jquery',
    'window.jQuery': 'jquery',
    Popper: ['popper.js', 'default']
});

// Extract compiled CSS into a seperate file
const extractCss = new MiniCssExtractPlugin({
    filename: '[name].css'
});

if (process.env.NODE_ENV) {
    module.exports.mode = process.env.NODE_ENV === 'production' ? 'production' : 'development';
}

module.exports = {
    devtool: 'inline-source-map',
    entry: {
        'site': ['./js/site.js'],
        'theme': ['./js/theme.js'],

        // pages
        'shared/error': './js/pages/shared/error.js',
        'upload/tracker': './js/pages/upload/tracker.js',
        'upload/index': './js/pages/upload/index.js'
    },
    plugins: [
        globals,
        extractCss
    ],
    optimization: {
        splitChunks: {
            cacheGroups: {
                'lib.js': {
                    test: /node_modules/,
                    chunks: 'initial',
                    name: 'lib',
                    enforce: true
                }
            },
        },
    },
    output: {
        filename: '[name].js',
        chunkFilename: '[name]',
        path: targetDir,
        publicPath: '/build/'
    },
    module: {
        rules: [
            {
                test: /\.(png|svg|jpe?g|gif|ttf|eot|woff|woff2)$/,
                type: 'asset'
            },
            {
                test: /\.css$/,
                use: [
                    process.env.NODE_ENV === 'development' ? 'style-loader' : MiniCssExtractPlugin.loader,
                    {
                        loader: 'css-loader',
                        options: {
                            sourceMap: true
                        }
                    }
                ]
            }
        ]
    }
};

