using System.ComponentModel;
using fyp_MDPHelperApp.Services;
using fyp_MDPHelperApp.ViewModels;

namespace fyp_MDPHelperApp.Views.About;

public class WebViewConfig
{
    public string Title { get; set; }
    public string HtmlContent { get; set; }
    public int LineCount { get; set; }  // Used to calculate height based on lines of code
}

public partial class HWTestingGuidePage : ContentPage
{
    private static readonly List<string> _title = new()
    {
        //must be same as the label's text
        "Flashing Methods",
        "Flash using STLink",
        "Flash using Uart",
        "Hardware Testing",
        "Data communication",
        "Motor Testing",
        "Servo Testing",
        "Ultrasonic Sensor Testing",
        "IR Sensor Testing",
        "Motion Sensor Testing",
        "Custom Testing"
    };

    private bool _showSearchResult;

    private const int HEIGHT_PER_LINE = 22; 
    public HWTestingGuidePage()
    {
        InitializeComponent();

        BindingContext = WebNavigationViewModel.Instance;

        // Subscribe to IsDarkMode change
        ThemeViewModel.Instance.PropertyChanged += OnThemeChanged;

        SetWebViews();
    }

    private void OnThemeChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeViewModel.Instance.IsDarkTheme))
        {
	        SetWebViews();
        }
    }

    /*UI components START*/
    private async void OnBackToTopClicked(object sender, EventArgs e)
    {
        await UserGuideScrollView.ScrollToAsync(0, 0, true); // Scrolls to the top
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchBar = (SearchBar)sender;
        var results = GetSearchResults(searchBar.Text);
        SearchStackLayout.HeightRequest = results.Count * 30 + 45;
        SearchResults.ItemsSource = results;
        _showSearchResult = results.Count > 0;
        SearchResults.IsVisible = _showSearchResult;
    }

    private void OnSearchResultTapped(object sender, ItemTappedEventArgs e)
    {
        if (e.Item != null && e.Item is string itemText) ScrollToSection(itemText);
    }

    private void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        ScrollToSection(SearchBar.Text);
    }
    /*UI components END*/

    /*Helper function START*/
    private void ScrollToSection(string searchTerm)
    {
        // Normalize the search term for case-insensitive matching
        var normalizedSearchTerm = searchTerm.ToLower();

        if (normalizedSearchTerm.Contains("flashing methods"))
            _ = UserGuideScrollView.ScrollToAsync(FlashingMethodsSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("flash using stlink"))
            _ = UserGuideScrollView.ScrollToAsync(STLinkFlash, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("flash using uart"))
            _ = UserGuideScrollView.ScrollToAsync(UartFlash, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("motor"))
            _ = UserGuideScrollView.ScrollToAsync(MotorTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("servo"))
            _ = UserGuideScrollView.ScrollToAsync(ServoTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("hardware"))
            _ = UserGuideScrollView.ScrollToAsync(HardwareTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("data"))
            _ = UserGuideScrollView.ScrollToAsync(DataCommSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("custom"))
            _ = UserGuideScrollView.ScrollToAsync(CustomTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("ultrasonic"))
            _ = UserGuideScrollView.ScrollToAsync(UltrasonicTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("ir"))
            _ = UserGuideScrollView.ScrollToAsync(IRTestingSection, ScrollToPosition.Start, true);
        else if (normalizedSearchTerm.Contains("motion"))
            _ = UserGuideScrollView.ScrollToAsync(MotionTestingSection, ScrollToPosition.Start, true);
        // Add more sections as needed
    }

    public static List<string> GetSearchResults(string query)
    {
        if (string.IsNullOrEmpty(query))
            return new List<string>();

        return _title.Where(item => item.ToLower().Contains(query.ToLower())).ToList();
    }

    private void OnScrolled(object sender, ScrolledEventArgs e)
    {
        if (e.ScrollY > 100)
        {
            BackToTopButton.IsVisible = true;
            SearchFrame.IsVisible = false;
            SearchBar.IsVisible = false;
            SearchResults.IsVisible = false;
        }
        else
        {
            BackToTopButton.IsVisible = false;
            SearchFrame.IsVisible = true;
            SearchBar.IsVisible = true;
            SearchResults.IsVisible = _showSearchResult;
        }
    }

    private readonly Dictionary<string, WebViewConfig> _webViewConfigs = new()
    {
        { "Motion", new WebViewConfig
            {
                Title = "Motion Sensor Testing",
                LineCount = 25,
                HtmlContent = @"
// Global variable : Update these elsewhere in your code
float gyroRollAngle = 0;
float accelPitchAngle = 0;
float leftRPM = 0;

/* Task to handle UART transmission */
void runUartTask(void *argument)
{
    uint32_t millisNow, lastSendTime = 0;  
    char message[100];  

    for (;;) // Infinite loop for continuous transmission
    {
        millisNow = HAL_GetTick();
        // Adjust sampling rate by modifying the interval : e.g. 500ms
        if (millisNow - lastSendTime >= 500)  
        {  
            // Format the data string based on your required fields
            snprintf(message, sizeof(message), 
            ""gyro roll angle: %.2f, accel pitch angle: %.2f, Left_RPM: %.2f\n"",
            gyroRollAngle, accelPitchAngle, leftRPM);

            // Transmit the message via UART
            HAL_UART_Transmit(&huart1, (uint8_t*)message, strlen(message), HAL_MAX_DELAY);
            lastSendTime = millisNow;  
        }
    }
}"
            }
        },
        { "Motion1", new WebViewConfig
	        {
		        Title = "Motion Sensor Testing",
		        LineCount = 14,
		        HtmlContent = @"
void get_angle_accelAll(float *angle, float accel[3]) { 
	
	float pitch_rad = atan(accel[0] / sqrt(pow(accel[1], 2) + pow(accel[2], 2)));
	float roll_rad = atan(accel[1] / sqrt(pow(accel[0], 2) + pow(accel[2], 2)));

    // Yaw cannot be calculated from accelerometer alone because accelerometer measures linear acceleration,
    // not rotational motion along the Z-axis. Yaw typically requires a magnetometer or a gyroscope.
    //tilt angle ensures the overall deviation of an object from the vertical axis (Z-axis) without specifying the direction
	float tilt_rad = atan(sqrt(pow(accel[0], 2) + pow(sensors_ptr->accel[1], 2)) / sensors_ptr->accel[2]);

    // Convert radians to degrees
    angle[0] = roll_rad * (180.0 / M_PI) - accel_bias_angle[0];   // Roll in degrees
    angle[1] = pitch_rad * (180.0 / M_PI) - accel_bias_angle[1];   // Pitch in degrees
    angle[2] = tilt_rad * (180.0 / M_PI)  - accel_bias_angle[2];   // tile in degrees 
}
"
	        }
        },
        { "Motion2", new WebViewConfig
	        {
		        Title = "Motion Sensor Testing",
		        LineCount = 9,
		        HtmlContent = @"
float get_angle_heading(float heading[3]) {
    float magX = heading[0];
    float magY = heading[1];

    // Compute heading (yaw) angle from magnetometer readings
    float headingAngle = atan2(magY, magX) * 180 / M_PI;

	return headingAngle;
}
"
	        }
        },
        { "Motion3", new WebViewConfig
	        {
		        Title = "Motion Sensor Testing",
		        LineCount = 10,
		        HtmlContent = @"
void get_angle_gyroAll(float timeElapsedInSec, float *gyro_angle, float gyro[3]){
	gyro_angle[0] += gyro[0] * timeElapsedInSec; // Roll
	gyro_angle[1] += gyro[1] * timeElapsedInSec; // Pitch
	gyro_angle[2] += gyro[2] * timeElapsedInSec; // Yaw

	// Ensure angles are within the range of -180 to 180 degrees
	gyro_angle[0] = fmod(gyro_angle[0], 360.0);
	gyro_angle[1] = fmod(gyro_angle[1], 360.0);
	gyro_angle[2] = fmod(gyro_angle[2], 360.0);
}
"
	        }
        },
        { "Motor1", new WebViewConfig
            {
                Title = "Motor Testing",
                LineCount = 62,
                HtmlContent = @"
//the timer that used to control the motors, the init code can be generated by STM32CudeIDE
TIM_HandleTypeDef htim8;

static void MX_TIM8_Init(void) {

	TIM_ClockConfigTypeDef sClockSourceConfig = { 0 };
	TIM_MasterConfigTypeDef sMasterConfig = { 0 };
	TIM_OC_InitTypeDef sConfigOC = { 0 };
	TIM_BreakDeadTimeConfigTypeDef sBreakDeadTimeConfig = { 0 };

	htim8.Instance = TIM8;
	htim8.Init.Prescaler = 0;
	htim8.Init.CounterMode = TIM_COUNTERMODE_UP;
	htim8.Init.Period = 7199;
	htim8.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
	htim8.Init.RepetitionCounter = 0;
	htim8.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
	if (HAL_TIM_Base_Init(&htim8) != HAL_OK) {
		Error_Handler();
	}
	sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
	if (HAL_TIM_ConfigClockSource(&htim8, &sClockSourceConfig) != HAL_OK) {
		Error_Handler();
	}
	if (HAL_TIM_PWM_Init(&htim8) != HAL_OK) {
		Error_Handler();
	}
	sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
	sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
	if (HAL_TIMEx_MasterConfigSynchronization(&htim8, &sMasterConfig)
			!= HAL_OK) {
		Error_Handler();
	}
	sConfigOC.OCMode = TIM_OCMODE_PWM1;
	sConfigOC.Pulse = 0;
	sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
	sConfigOC.OCNPolarity = TIM_OCNPOLARITY_HIGH;
	sConfigOC.OCFastMode = TIM_OCFAST_DISABLE;
	sConfigOC.OCIdleState = TIM_OCIDLESTATE_RESET;
	sConfigOC.OCNIdleState = TIM_OCNIDLESTATE_RESET;
	if (HAL_TIM_PWM_ConfigChannel(&htim8, &sConfigOC, TIM_CHANNEL_1)
			!= HAL_OK) {
		Error_Handler();
	}
	if (HAL_TIM_PWM_ConfigChannel(&htim8, &sConfigOC, TIM_CHANNEL_2)
			!= HAL_OK) {
		Error_Handler();
	}
	sBreakDeadTimeConfig.OffStateRunMode = TIM_OSSR_DISABLE;
	sBreakDeadTimeConfig.OffStateIDLEMode = TIM_OSSI_DISABLE;
	sBreakDeadTimeConfig.LockLevel = TIM_LOCKLEVEL_OFF;
	sBreakDeadTimeConfig.DeadTime = 0;
	sBreakDeadTimeConfig.BreakState = TIM_BREAK_DISABLE;
	sBreakDeadTimeConfig.BreakPolarity = TIM_BREAKPOLARITY_HIGH;
	sBreakDeadTimeConfig.AutomaticOutput = TIM_AUTOMATICOUTPUT_DISABLE;
	if (HAL_TIMEx_ConfigBreakDeadTime(&htim8, &sBreakDeadTimeConfig)
			!= HAL_OK) {
		Error_Handler();
	}

	HAL_TIM_MspPostInit(&htim8);

}
// Set motor duty cycle using timer PWM
void SetMotorDuty(TIM_HandleTypeDef* timer, uint16_t dutyL, uint16_t dutyR)
{
    timer->Instance->CCR1 = dutyL; // Set left motor PWM duty cycle
    timer->Instance->CCR2 = dutyR; // Set right motor PWM duty cycle
}

//example to set the duty cycle of the motors
SetMotorDuty(&htim8, 1000, 1000);
"
            }
        },{ "Motor2", new WebViewConfig
            {
                Title = "Motor Testing",
                LineCount = 72,
                HtmlContent = @"
//the timer that used to control encoder, the init code can be generated by STM32CudeIDE
static void MX_TIM2_Init(void) {
	TIM_Encoder_InitTypeDef sConfig = { 0 };
	TIM_MasterConfigTypeDef sMasterConfig = { 0 };

	htim2.Instance = TIM2;
	htim2.Init.Prescaler = 0;
	htim2.Init.CounterMode = TIM_COUNTERMODE_UP;
	htim2.Init.Period = 65535;
	htim2.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
	htim2.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_DISABLE;
	sConfig.EncoderMode = TIM_ENCODERMODE_TI12;
	sConfig.IC1Polarity = TIM_ICPOLARITY_RISING;
	sConfig.IC1Selection = TIM_ICSELECTION_DIRECTTI;
	sConfig.IC1Prescaler = TIM_ICPSC_DIV1;
	sConfig.IC1Filter = 0;
	sConfig.IC2Polarity = TIM_ICPOLARITY_RISING;
	sConfig.IC2Selection = TIM_ICSELECTION_DIRECTTI;
	sConfig.IC2Prescaler = TIM_ICPSC_DIV1;
	sConfig.IC2Filter = 0;
	if (HAL_TIM_Encoder_Init(&htim2, &sConfig) != HAL_OK) {
		Error_Handler();
	}
	sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
	sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
	if (HAL_TIMEx_MasterConfigSynchronization(&htim2, &sMasterConfig)
			!= HAL_OK) {
		Error_Handler();
	}
}

//timer 3 setup is same as timer 2...

void encoder_task(void *argument) {
	/* USER CODE BEGIN encoder_task */
	// Start encoder timers
	HAL_TIM_Encoder_Start(&htim2, TIM_CHANNEL_ALL);
	HAL_TIM_Encoder_Start(&htim3, TIM_CHANNEL_ALL);

	uint32_t tick = HAL_GetTick();

	uint16_t cntL_prev = 0;
	uint16_t cntR_prev = 0;

	int TIMER_MAX = 65535;  // 16-bit counter max value

	for (;;) {
		if (HAL_GetTick() - tick >= 1000L)  // update every 1 second
		{
			// Read current encoder counts
			uint16_t cntL_now = __HAL_TIM_GET_COUNTER(&htim2);
			uint16_t cntR_now = __HAL_TIM_GET_COUNTER(&htim3);

			// Handle overflow conditions :16-bit counter wraparound
			int diffL = (int) (cntL_now - cntL_prev);
			int diffR = (int) (cntR_now - cntR_prev);

			if (diffL > TIMER_MAX / 2) {
				diffL -= TIMER_MAX;
			} else if (diffL < -TIMER_MAX / 2) {
				diffL += TIMER_MAX;
			}

			if (diffR > TIMER_MAX / 2) {
				diffR -= TIMER_MAX;
			} else if (diffR < -TIMER_MAX / 2) {
				diffR += TIMER_MAX;
			}

			// Calculate RPM using the correct PPR
			rpmL = (diffL * 60) / (PPR * 4); // 4x counts per pulse in quadrature mode
			rpmR = (diffR * 60) / (PPR * 4); // PPR = 330, according to datasheet

			// Update previous encoder counts
			cntL_prev = cntL_now;
			cntR_prev = cntR_now;

			// Reset tick
			tick = HAL_GetTick();
		}
		osDelay(250);   
	}
	/* USER CODE END encoder_task */
}"
            }
        },
        {
            "Servo1",new WebViewConfig
            {
                Title = "Servo Testing",
                LineCount = 56,
                HtmlContent = @"
//the timer that used to control the servo, the init code can be generated by STM32CudeIDE
TIM_HandleTypeDef htim1;

static void MX_TIM1_Init(void) {
	TIM_ClockConfigTypeDef sClockSourceConfig = { 0 };
	TIM_MasterConfigTypeDef sMasterConfig = { 0 };
	TIM_OC_InitTypeDef sConfigOC = { 0 };
	TIM_BreakDeadTimeConfigTypeDef sBreakDeadTimeConfig = { 0 };

	htim1.Instance = TIM1;
	htim1.Init.Prescaler = 160;
	htim1.Init.CounterMode = TIM_COUNTERMODE_UP;
	htim1.Init.Period = 1000;
	htim1.Init.ClockDivision = TIM_CLOCKDIVISION_DIV1;
	htim1.Init.RepetitionCounter = 0;
	htim1.Init.AutoReloadPreload = TIM_AUTORELOAD_PRELOAD_ENABLE;
	if (HAL_TIM_Base_Init(&htim1) != HAL_OK) {
		Error_Handler();
	}
	sClockSourceConfig.ClockSource = TIM_CLOCKSOURCE_INTERNAL;
	if (HAL_TIM_ConfigClockSource(&htim1, &sClockSourceConfig) != HAL_OK) {
		Error_Handler();
	}
	if (HAL_TIM_PWM_Init(&htim1) != HAL_OK) {
		Error_Handler();
	}
	sMasterConfig.MasterOutputTrigger = TIM_TRGO_RESET;
	sMasterConfig.MasterSlaveMode = TIM_MASTERSLAVEMODE_DISABLE;
	if (HAL_TIMEx_MasterConfigSynchronization(&htim1, &sMasterConfig)
			!= HAL_OK) {
		Error_Handler();
	}
	sConfigOC.OCMode = TIM_OCMODE_PWM1;
	sConfigOC.Pulse = 0;
	sConfigOC.OCPolarity = TIM_OCPOLARITY_HIGH;
	sConfigOC.OCFastMode = TIM_OCFAST_ENABLE;
	sConfigOC.OCIdleState = TIM_OCIDLESTATE_RESET;
	sConfigOC.OCNIdleState = TIM_OCNIDLESTATE_RESET;
	if (HAL_TIM_PWM_ConfigChannel(&htim1, &sConfigOC, TIM_CHANNEL_4)
			!= HAL_OK) {
		Error_Handler();
	}
	sBreakDeadTimeConfig.OffStateRunMode = TIM_OSSR_DISABLE;
	sBreakDeadTimeConfig.OffStateIDLEMode = TIM_OSSI_DISABLE;
	sBreakDeadTimeConfig.LockLevel = TIM_LOCKLEVEL_OFF;
	sBreakDeadTimeConfig.DeadTime = 0;
	sBreakDeadTimeConfig.BreakState = TIM_BREAK_DISABLE;
	sBreakDeadTimeConfig.BreakPolarity = TIM_BREAKPOLARITY_HIGH;
	sBreakDeadTimeConfig.AutomaticOutput = TIM_AUTOMATICOUTPUT_DISABLE;
	if (HAL_TIMEx_ConfigBreakDeadTime(&htim1, &sBreakDeadTimeConfig)
			!= HAL_OK) {
		Error_Handler();
	}

	HAL_TIM_MspPostInit(&htim1);
}

// Set servo duty cycle using timer PWM
void SetServoTurn(TIM_HandleTypeDef* timer, uint16_t servoDutyCycle)
{
    timer->Instance->CCR4 = servoDutyCycle;
}

//example to set the duty cycle of the servo
SetServoTurn(&htim1, 130);
"
            }
        }
    };

    private string HighlightSyntax(string code)
    {
        // Highlight keywords, comments, and strings using regex
        code = System.Text.RegularExpressions.Regex.Replace(code, @"\b(float|uint32_t|char|void|for|if|else|return|int|uint16_t)\b", @"<span class='keyword'>$1</span>");
        code = System.Text.RegularExpressions.Regex.Replace(code, @"(//.*)", @"<span class='comment'>$1</span>");
        code = System.Text.RegularExpressions.Regex.Replace(code, @"""(.*?)""", @"<span class='string'>""$1""</span>");
        code = System.Text.RegularExpressions.Regex.Replace(code, @"\b([a-zA-Z_]\w*)\s*\(", @"<span class='function'>$1</span>(");

        return code;
    }

    
    private void SetWebViewContent(WebView webView, WebViewConfig config)
    {
        if (config == null) return;

        webView.HeightRequest = config.LineCount * HEIGHT_PER_LINE;

        webView.Source = new HtmlWebViewSource
        {
            Html = $@"
<html><head><style>
    body {{
        font-family: 'Courier New', monospace;
        font-size: 14px;
        color: black;
        background-color: white;
        margin: 0;
        padding: 0;
        height: auto;
        overflow-x: hidden;
        white-space: pre-wrap;
    }}
    pre {{
        margin: 0;
        padding: 10px;
        white-space: pre-wrap;
        word-wrap: break-word;
        overflow-x: auto;
    }}
    /* Syntax Highlighting */
    .comment {{ color: green; }}
    .keyword {{ color: red; font-weight: bold; }}
    .function {{ color: purple; font-weight: bold; }}
    .string {{ color: #0047AB; }}

    /* Dark Mode */
    body.dark-theme {{
        background-color: black;
        color: white;
    }}
</style></head><body class='{(ThemeViewModel.Instance.IsDarkTheme ? "dark-theme" : "light-theme")}'><pre>{HighlightSyntax(config.HtmlContent)}</pre></body></html>"
        };
    }

    private void SetWebViews()
    {
        SetWebViewContent(WebViewMotion, _webViewConfigs["Motion"]);
        SetWebViewContent(WebViewMotion1, _webViewConfigs["Motion1"]);
        SetWebViewContent(WebViewMotion2, _webViewConfigs["Motion2"]);
        SetWebViewContent(WebViewMotion3, _webViewConfigs["Motion3"]);
        
        SetWebViewContent(WebViewMotor1, _webViewConfigs["Motor1"]);
        SetWebViewContent(WebViewMotor2, _webViewConfigs["Motor2"]);
        
        SetWebViewContent(WebViewServo1, _webViewConfigs["Servo1"]);
    }

    /*Helper function END*/
}