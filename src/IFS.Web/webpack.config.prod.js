/// <binding />
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');

const webpack = require('webpack');

module.exports = {
    stats: {chunkModules: true},
    optimization: {
        minimizer: [
            // Minification
            new UglifyJsPlugin({
                parallel: true,
                uglifyOptions: {
                    compress: {
                        dead_code: true,
                        drop_console: true,
                        drop_debugger: true,
                        global_defs: {
                            DEBUG: false,
                            "module.hot": false,
                        },
                        passes: 2,
                        warnings: true,
                    },
                    output: {
                        beautify: false,
                    },
                    ecma: 5,
                },
                warningsFilter: () => true,
            })
        ]
    }
};