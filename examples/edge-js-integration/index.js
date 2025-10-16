const edge = require('edge-js');
const path = require('path');

class LinksClient {
    constructor(dataFilePath) {
        const assemblyPath = path.join(__dirname, 'bin', 'Release', 'net8.0', 'EdgeJsIntegration.dll');

        // Load the .NET assembly
        this._createLink = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'CreateLink'
        });

        this._getOrCreate = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'GetOrCreate'
        });

        this._update = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'Update'
        });

        this._delete = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'Delete'
        });

        this._getLink = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'GetLink'
        });

        this._count = edge.func({
            assemblyFile: assemblyPath,
            typeName: 'EdgeJsIntegration.LinksAdapter',
            methodName: 'Count'
        });

        // Initialize adapter with data file path
        this.dataFilePath = dataFilePath;
        this._initializeAdapter(dataFilePath);
    }

    _initializeAdapter(dataFilePath) {
        // Constructor initialization happens on first method call
        this._initialized = false;
        this._dataFilePath = dataFilePath;
    }

    async createLink(source, target) {
        const result = await this._createLink({ source, target });
        return result;
    }

    async getOrCreate(source, target) {
        const result = await this._getOrCreate({ source, target });
        return result;
    }

    async update(link, newSource, newTarget) {
        const result = await this._update({ link, newSource, newTarget });
        return result;
    }

    async delete(link) {
        const result = await this._delete({ link });
        return result;
    }

    async getLink(link) {
        const result = await this._getLink({ link });
        return result;
    }

    async count() {
        const result = await this._count({});
        return result;
    }
}

module.exports = LinksClient;
