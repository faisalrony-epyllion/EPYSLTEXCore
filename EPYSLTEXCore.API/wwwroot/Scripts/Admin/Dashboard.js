(function () {
    var menuId;

    $(function () {
        if (!menuId) menuId = localStorage.getItem("menuId");

        readyOperation();
    });

    function readyOperation() {
        $('.ej2-datepicker-dashboard').each(function (i, el) {
            $(el).datepicker({
                todayHighlight: true,
                format: "dd-M-yyyy",
                autoclose: true,
                todayBtn: "linked"
            }).on("show", function (date) {
                if (this.value && !date.date) {
                    $(this).datepicker('update', this.value);
                }
            });
        });
    }
})();

