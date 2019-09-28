/// <binding />
const TerserPlugin = require('terser-webpack-plugin');

const webpack = require('webpack');

module.exports = {
    stats: {chunkModules: true},
    optimization: {
        minimizer: [
            // Minification
            new TerserPlugin({
                terserOptions: {
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
                }
            })
        ]
    }
};