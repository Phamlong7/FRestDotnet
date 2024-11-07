$(document).ready(function () {
    $(document).on('click', '.btn-add-cart', function () {
        $.blockUI(); // Block UI during AJAX call

        const id = $(this).data('dish-id'); // Get the dish ID (Ensure this matches your button's data attribute)
        console.log("Adding to cart: " + id); // Log the ID being added

        if (!id) {
            console.error("Dish ID not found!");
            $.unblockUI(); // Unblock UI if there's an error
            return;
        }

        $.ajax({
            url: '/cart/add', // Ensure this URL matches your route
            method: 'POST',
            data: { DishId: id, Quantity: 1 }, // Ensure property names match your CartModel
            success: function (count) {
                console.log("Response count: " + count); // Log the response count
                if (count === -1) {
                    showToast('Error adding item to cart', 'error'); // Show error message
                } else {
                    $('.cart-number').text(count); // Update the cart number in the navbar
                    showToast('Dish added to cart successfully!', 'success'); // Show success message
                }
                $.unblockUI(); // Unblock UI after AJAX call
            },
            error: function (xhr, status, error) {
                console.error("AJAX error: " + status + " - " + error); // Log AJAX error
                showToast('Failed to add dish to cart', 'error'); // Handle AJAX error
                $.unblockUI(); // Unblock UI if there's an error
            }
        });
    });


    function showToast(message, type) {
        // Create a div for the toast and add text
        const toast = $('<div></div>').text(message);

        // Add inline styles for positioning and appearance
        toast.css({
            "position": "fixed",
            "top": "20px", // Position it at the bottom
            "right": "20px", // Position it at the right corner
            "background-color": (type === 'success' ? '#28a745' : '#dc3545'), // Green for success, Red for error
            "color": "#fff", // White text color
            "padding": "15px",
            "border-radius": "5px",
            "z-index": 10000, // High z-index to ensure it appears on top
            "opacity": "0.9",
            "font-size": "16px",
            "box-shadow": "0 2px 10px rgba(0,0,0,0.1)"
        });

        // Append the toast to the body
        $('body').append(toast);

        // Fade out the toast after 3 seconds
        setTimeout(() => {
            toast.fadeOut(() => {
                toast.remove(); // Remove the toast after it fades out
            });
        }, 3000); // 3-second delay before fading out
    }
});

document.getElementById('cart-toggle').addEventListener('click', function (e) {
    e.preventDefault();
    const cartDropdown = document.getElementById('cart-dropdown');
    const isLoggedIn = document.body.getAttribute('data-logged-in') === "True"; // Check login status

    if (!isLoggedIn) {
        // If the user is not logged in, redirect to login page with ReturnUrl set to /Cart
        window.location.href = "/Account/Login?ReturnUrl=%2FCart";
        return;
    }

    // Toggle visibility of the cart dropdown if the user is logged in
    cartDropdown.style.display = (cartDropdown.style.display === 'block') ? 'none' : 'block';
    cartDropdown.style.width = '300px';
    cartDropdown.style.maxHeight = '550px';
    cartDropdown.style.padding = '20px';

    // Fetch and display cart items from the partial view
    fetch('/Cart/GetCartItems')
        .then(response => response.text())
        .then(html => {
            const cartItemsContainer = document.querySelector('.cart-items');
            cartItemsContainer.innerHTML = html;  // Directly insert the HTML from the partial

            // Reattach event listeners for remove buttons after loading items
            attachRemoveItemEventListeners();

            // Show or hide footer based on cart item count
            toggleFooterVisibility();
        })
        .catch(error => console.error('Error fetching cart items:', error));
});

// Function to attach the remove item event listeners
function attachRemoveItemEventListeners() {
    document.querySelectorAll('.remove-item').forEach(button => {
        button.addEventListener('click', function () {
            const dishId = this.getAttribute('data-id');

            fetch(`/Cart/Remove?dishId=${dishId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // Remove the cart item from the DOM
                        this.closest('.cart-item').remove();

                        // Update the cart number
                        document.querySelector('.cart-number').textContent = data.numberCart;

                        // Recalculate the total amount based on remaining items
                        updateTotalAmount();

                        // Show or hide footer based on cart item count
                        toggleFooterVisibility();

                        // If the cart is empty, display the empty message
                        if (data.isEmpty) {
                            document.querySelector('.cart-items').innerHTML = `
                            <div class="cart-title" style="font-size: 18px; font-weight: bold; margin-bottom: 10px; color: #333;">
                             Your Cart
                            </div>
<hr style="border-top: 1px solid #ddd; margin-bottom: 15px;" />
                            <div class="empty-cart-message" style="text-align: center; color: #555; margin-top: 10px;">
                                There are no items in your cart.
                            </div>`;
                        }
                    }
                })
                .catch(error => console.error('Error removing item:', error));
        });
    });
}

// Function to recalculate the total amount and toggle footer visibility
function updateTotalAmount() {
    let total = 0;
    document.querySelectorAll('.cart-item').forEach(item => {
        const itemPrice = parseFloat(item.getAttribute('data-price'));
        const itemQuantity = parseInt(item.querySelector('.cart-item-quantity').textContent.replace('Quantity: ', ''));
        total += itemPrice * itemQuantity;
    });

    // Format total with comma as thousands separator
    document.getElementById('total-amount').textContent = `$${total.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;

    // Show or hide footer based on cart item count
    toggleFooterVisibility();

    // Reattach the remove event listeners after total update
    attachRemoveItemEventListeners();
}

// Function to show/hide footer based on cart item presence
function toggleFooterVisibility() {
    const footer = document.getElementById('dropdown-footer');
    const hasItems = document.querySelectorAll('.cart-item').length > 0;
    footer.style.display = hasItems ? 'block' : 'none';
}

// Initial call to attach event listeners when the page loads
document.addEventListener('DOMContentLoaded', attachRemoveItemEventListeners);
