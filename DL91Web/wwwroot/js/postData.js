var postData = {
    open: function (url,data) {
        var blob = new Blob([JSON.stringify(data)], { type: 'application/json' });
        var burl = URL.createObjectURL(blob);
        window.open(url + "#" + burl.split('/').pop(), "_blank");
    },
    getData: function () {
        var code = window.location.href.split("#").pop();
        var data = sessionStorage.getItem(code);
        if (!data) {
            $.ajax({
                async: false,
                url: 'blob:'
                    + window.location.origin
                    + '/' + code,
                type: "GET",
                dataType: "json",
                success: function (_d) {
                    data = _d;
                    sessionStorage.setItem(code, JSON.stringify(data));
                },
                error: function (xhr, ajaxOptions, thrownError) {
                    alert(xhr.status);
                    alert(thrownError);
                }
            });
        }
        else {
            data = JSON.parse(data);
        }
        return data;
    }
}