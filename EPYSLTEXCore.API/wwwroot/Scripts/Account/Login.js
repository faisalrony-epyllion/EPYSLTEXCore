$(function () {

    loadProgressBar();

    $("#btn-signin").on('click', function (e) {
        $(this).addClass("disabled");
        e.preventDefault();

        var data = $("#login-form").serialize();
    
        axios.post('/account/login', data)

            .then(function (response) {

                localStorage.setItem("token", response.data.accessToken);
                window.location.href = "/dashboard/index";
            })
            .catch(function () {
                toastr.error('Invalid username or password!', 'Error');
                $("#btn-signin").removeClass("disabled");
            });
    });






});
function changeForm(change) {

    if (change == 0) {

        $("#forgot-password").show();
        $("#forgot-span").show();

        $("#sign-in").hide();
        $("#sign-in-span").hide();
    }
    else {
        $("#sign-in").show();
        $("#sign-in-span").show();

        $("#forgot-span").hide();
        $("#forgot-password").hide();
    }

}