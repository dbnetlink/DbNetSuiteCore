"use strict";
class DbNetFile extends DbNetSuite {
    constructor(id) {
        super(id);
        this.folder = "";
    }
    initialize() {
        if (!this.element) {
            return;
        }
        this.element.empty();
        this.folderPanel = this.addPanel("folder");
        this.addLoadingPanel();
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    reload() {
        this.callServer("page");
    }
    configurePage(response) {
        var _a;
        (_a = this.folderPanel) === null || _a === void 0 ? void 0 : _a.html(response.html);
        this.fireEvent("onPageLoaded", {});
    }
    callServer(action) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configurePage(response);
            }
        });
    }
    getRequest() {
        const request = {};
        request.folder = this.folder;
        return request;
    }
}
