/// <binding />
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

const webpack = require('webpack');

module.exports = {
    stats: 'detailed',
    plugins: [
        // Ensure modules name change when the contents change (cache busting)
        new webpack.HashedModuleIdsPlugin({
            hashFunction: 'sha256',
            hashDigest: 'hex',
            hashDigestLength: 20
        }),

        // Minification
        new UglifyJsPlugin({
            parallel: true,
            compress: {
                dead_code: true,
                drop_console: true,
                drop_debugger: true,
                global_defs: {
                    DEBUG: false,
                    'module.hot': false
                },
                passes: 2,
                pure_funcs: [
                    'console.log',
                    'console.info'
                ],
                warnings: true
            },
            output: {
                beautify: false
            },
            ecma: 5,
            warnings: true
        })
    ]
};