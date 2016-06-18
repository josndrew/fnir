################################################################################
# Automatically-generated file. Do not edit!
################################################################################

-include ../makefile.init

RM := rm -rf

# All of the sources participating in the build are defined here
-include sources.mk
-include subdir.mk
-include objects.mk

ifneq ($(MAKECMDGOALS),clean)
ifneq ($(strip $(ASM_DEPS)),)
-include $(ASM_DEPS)
endif
ifneq ($(strip $(S_DEPS)),)
-include $(S_DEPS)
endif
ifneq ($(strip $(S_UPPER_DEPS)),)
-include $(S_UPPER_DEPS)
endif
ifneq ($(strip $(C_DEPS)),)
-include $(C_DEPS)
endif
endif

-include ../makefile.defs

# Add inputs and outputs from these tool invocations to the build variables 
LSS += \
micro.lss \

FLASH_IMAGE += \
micro.hex \

EEPROM_IMAGE += \
micro.eep \

SIZEDUMMY += \
sizedummy \

AVRDUDEDUMMY += \
avrdudedummy \


# All Target
all: micro.elf secondary-outputs

# Tool invocations
micro.elf: $(OBJS) $(USER_OBJS)
	@echo 'Building target: $@'
	@echo 'Invoking: AVR C Linker'
	avr-gcc -Wl,-Map,micro.map -mmcu=atmega328p -o "micro.elf" $(OBJS) $(USER_OBJS) $(LIBS)
	@echo 'Finished building target: $@'
	@echo ' '

micro.lss: micro.elf
	@echo 'Invoking: AVR Create Extended Listing'
	-avr-objdump -h -S micro.elf  >"micro.lss"
	@echo 'Finished building: $@'
	@echo ' '

micro.hex: micro.elf
	@echo 'Create Flash image (ihex format)'
	-avr-objcopy -R .eeprom -R .fuse -R .lock -R .signature -O ihex micro.elf  "micro.hex"
	@echo 'Finished building: $@'
	@echo ' '

micro.eep: micro.elf
	@echo 'Create eeprom image (ihex format)'
	-avr-objcopy -j .eeprom --no-change-warnings --change-section-lma .eeprom=0 -O ihex micro.elf  "micro.eep"
	@echo 'Finished building: $@'
	@echo ' '

sizedummy: micro.elf
	@echo 'Invoking: Print Size'
	-avr-size --format=avr --mcu=atmega328p micro.elf
	@echo 'Finished building: $@'
	@echo ' '

avrdudedummy: micro.elf
	@echo 'Invoking: AVRDude'
	C:\Users\Andrew Joseph\Downloads\XLoader\avrdude -pm328p -carduino -Uflash:w:micro.hex:a
	@echo 'Finished building: $@'
	@echo ' '

# Other Targets
clean:
	-$(RM) $(FLASH_IMAGE)$(ELFS)$(OBJS)$(ASM_DEPS)$(EEPROM_IMAGE)$(AVRDUDEDUMMY)$(S_DEPS)$(SIZEDUMMY)$(S_UPPER_DEPS)$(LSS)$(C_DEPS) micro.elf
	-@echo ' '

secondary-outputs: $(LSS) $(FLASH_IMAGE) $(EEPROM_IMAGE) $(SIZEDUMMY) $(AVRDUDEDUMMY)

.PHONY: all clean dependents
.SECONDARY:

-include ../makefile.targets
