/// <binding />
const path = require('path');
const webpack = require('webpack');
const targetDir = path.resolve(__dirname, 'wwwroot/build');
const ExtractTextPlugin = require('extract-text-webpack-plugin');

const globals = new webpack.ProvidePlugin({
    $: 'jquery',
    jQuery: 'jquery',
    'window.jQuery': 'jquery',
    Popper: ['popper.js', 'default']
});

// Extract compiled CSS into a seperate file
const extractCss = new ExtractTextPlugin({
    filename: 'site.css'
});

const libraries = [
    'jquery',
    'jquery-validation',
    'jquery-validation-unobtrusive',
    'popper.js',
    'bootstrap'
];

module.exports =  {
    devtool: 'inline-source-map',
    entry: {
        'site.js': ['./js/site.js'],

         // pages
        'shared/error.js': './js/pages/shared/error.js',
        'upload/tracker.js': './js/pages/upload/tracker.js',
        'upload/index.js': './js/pages/upload/index.js'
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
					chunks: "initial",
					name: "lib.js",
					enforce: true
				}
            },
        },
    },
    output: {
        filename: '[name]',
        chunkFilename: '[name]',
        path: targetDir,
        publicPath: '/build/'
    },
    module: {
        rules: [
            {
                test: /\.(png|svg|jpg|gif|ttf|eot|woff|woff2)$/,
                use: 'url-loader?limit=8192'
            },
            {
                test: /\.css$/,
                use: extractCss.extract({
                    use: [
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: true,
                            }
                        }
                    ],
                    fallback: 'style-loader'
                })
            }
        ]
    }
};

if (process.env.NODE_ENV) {
    module.exports.mode = process.env.NODE_ENV === 'production' ? 'production' : 'development';
}