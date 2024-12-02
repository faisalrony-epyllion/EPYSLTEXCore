$(function () {

    loadProgressBar();

    $("#btnChangeAcPassword").on('click', function (e) {
   
        e.preventDefault();

     

        if (!$("#changePasswordAcForm #OldPassword").val()) {
            toastr.error("Current password can't be null!", 'Error'); return;
        }

        if (!$("#changePasswordAcForm #NewPassword").val()) {
            toastr.error("New password can't be null!", 'Error'); return;
        }

        if (!$("#changePasswordAcForm #ConfirmPassword").val()) {
            toastr.error("Confirm password can't be null!", 'Error'); return;
        }

        if (!(/^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,20}$/.test($("#changePasswordAcForm #NewPassword").val()))) {
        
            toastr.error("Validation", 'At least one uppercase,one lower case,one numeric number and length between 6-20');
            return;
        }
      
        if ($("#changePasswordAcForm #ConfirmPassword").val() === $("#changePasswordAcForm #NewPassword").val()) {
            $(this).addClass("disabled");
            var data = $("#changePasswordAcForm").serialize();
            
            axios.post('/account/ChangePassword', data)

                .then(function (response) {
                
                    if (response.data.StatusCode == 200) {
                        toastr.success(response.data.message);
                    }
                    else
                        toastr.error(response.data.message, 'Error');
                })
                .catch(function () {
                    toastr.error('Invalid password!', 'Error');
                   
                });




        }
        else {        
            toastr.error("New and Confirm Password doesn't match !", 'Error');
        }
        $("#btnChangeAcPassword").removeClass("disabled");
    });

    $("#btnChangeEmPassword").on('click', function (e) {
      
        e.preventDefault();
   
        if (!$("#changePasswordEmForm #OldPassword").val()) {
            toastr.error("Current password can't be null!", 'Error'); return;
        }
      
        if (!$("#changePasswordEmForm #NewPassword").val()) {
            toastr.error("New password can't be null!", 'Error'); return;
        }
   
        if (!$("#changePasswordEmForm #ConfirmPassword").val()) {
            toastr.error("Confirm password can't be null!", 'Error'); return;
        }

   
        
        if ($("#changePasswordEmForm #ConfirmPassword").val() === $("#changePasswordEmForm #NewPassword").val()) {
            $(this).addClass("disabled");
            var data = $("#changePasswordEmForm").serialize();
            
            axios.post('/account/ChangePassword', data)

                .then(function (response) {

                    if (response.data.StatusCode == 200) {
                        toastr.success(response.data.message);
                    }
                    else
                        toastr.error(response.data.message, 'Error');
                })
                .catch(function () {
                    toastr.error('Invalid password!', 'Error');

                });




        }
        else {
            toastr.error("New and Confirm Password doesn't match !", 'Error');
        }
        $("#btnChangeEmPassword").removeClass("disabled");
    });

    $('a[data-toggle="pill"]').on('shown.bs.tab', function (e) {
        document.getElementById('changePasswordAcForm').reset();
        document.getElementById('changePasswordEmForm').reset();
    });



});


function ResetForm(id) {

    if (id == 1)
        document.getElementById('changePasswordEmForm').reset();
    else 
        document.getElementById('changePasswordAcForm').reset();
}